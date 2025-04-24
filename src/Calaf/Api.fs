// Api. Composition Root
namespace Calaf

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain

module Api =    
    
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
    
    let private initProject projectFileInfo =
        let createProject metadata xml =            
            match Project.tryCreate xml metadata with
            | None -> CannotCreateProject metadata.Name |> Init |> Error
            | Some project -> (project, xml) |> Ok            
        result {
            let metadata = ProjectMetadata.create projectFileInfo            
            let! xml = Xml.TryLoadXml(metadata.AbsolutePath)            
            return! createProject metadata xml
        }
        
    let private saveProject (project, xml) newVersion =        
        result {
            let! bumpedProject, xml = Project.tryBump xml project newVersion
            match bumpedProject with
            | Bumped (metadata, lang, prevVer, curVer) ->
                let! xml = Xml.TrySaveXml metadata.AbsolutePath xml
                return! (Bumped (metadata, lang, prevVer, curVer), xml) |> Ok
            | Versioned (metadata, lang, ver) ->
                return! GivenNotBumpedProject metadata.Name
                        |> Api
                        |> Error
            | Unversioned (metadata, lang) ->
                return! GivenUnversionedProject metadata.Name
                        |> Api
                        |> Error            
        }
        
    let initWorkspace workingDir =                
        result {
            let! dir = workingDir
                                |> FileSystem.TryGetDirectory            
            let! files = dir
                                |> FileSystem.TryReadFiles supportedFilesPattern
                
            let projects, errors = files
                                |> Seq.map initProject
                                |> Result.partition           
            
            let! currentVer = projects
                                |> Seq.map fst
                                |> WorkspaceVersion.create
                                |> fun x -> x.PropertyGroup |> Option.toResult (NoPropertyGroupWorkspaceVersion |> Api)
                                
            let timeStamp = Clock.NowUtc
            let! bumpedVer = Version.tryBump currentVer timeStamp
                                |> Option.toResult (NoPropertyGroupNextVersion |> Api)
                                
            let projects, errors = projects
                                |> Seq.map (fun (project, xml) -> saveProject (project, xml) bumpedVer)
                                |> Result.partition
                                |> fun (p, e) -> (p |> Seq.map fst , errors |> Seq.append e)
                                
            return Workspace.create (dir, projects)
        }