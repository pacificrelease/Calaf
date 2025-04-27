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
    
    let private init projectFileInfo =
        let createProject metadata xml =            
            match Project.tryCreate xml metadata with
            | None -> CannotCreateProject metadata.Name |> Init |> Error
            | Some project -> (project, xml) |> Ok            
        result {
            let metadata = ProjectMetadata.create projectFileInfo            
            let! xml = Xml.tryLoadXml(metadata.AbsolutePath)            
            return! createProject metadata xml
        }
        
    let private apply (project, xml) newVersion =        
        result {
            let! bumped, xml = Project.tryBump xml project newVersion
            match bumped with
            | Bumped (metadata, lang, prevVer, curVer) ->
                let! xml = Xml.trySaveXml metadata.AbsolutePath xml
                return! (Bumped (metadata, lang, prevVer, curVer), xml) |> Ok
            | Versioned (metadata, _, _) ->
                return! GivenNotBumpedProject metadata.Name
                        |> Api
                        |> Error
            | Skipped (metadata, _, _) ->
                return! GivenSkippedProject metadata.Name
                        |> Api
                        |> Error
            | Unversioned (metadata, _) ->
                return! GivenUnversionedProject metadata.Name
                        |> Api
                        |> Error            
        }    
    
    let private save projects newVersion =        
        projects
        |> Seq.map (fun (project, xml) ->
            match project with
            | Versioned (_, lang, CalVer _) ->
                match apply (project, xml) newVersion with
                | Ok updated -> Ok updated
                | Error e    -> Error e
            | _ -> Ok (project, xml))
        
    let initWorkspace dir =                
        result {
            let! dir     = dir |> FileSystem.tryGetDirectory
            let! repo = dir |> Git.tryReadRepository
            let! files   = dir |> FileSystem.tryReadFiles supportedFilesPattern
            
            let projects,
                iErrors = files
                            |> Seq.map init
                            |> Result.partition           
            
            let! currentVer = projects
                            |> Seq.map fst
                            |> WorkspaceVersion.create
                            |> fun x -> x.PropertyGroup
                                        |> Option.toResult (NoPropertyGroupWorkspaceVersion |> Api)
                                
            let timeStamp = Clock.now()
            let! bumpedVer = Version.tryBump currentVer timeStamp.UtcDateTime
                            |> Option.toResult (NoPropertyGroupNextVersion |> Api)            
            let projects,
                sErrors = save projects bumpedVer
                            |> Result.partition
                                
            return Workspace.create (dir, projects |> Seq.map fst)
        }