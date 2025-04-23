// Api. Composition Root
namespace Calaf

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain

module Api =    
    
    [<Literal>]
    let private supportedDotNetFilesPattern = "*.?sproj"
    
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
            let! dirInfo =
                workingDir |> FileSystem.TryGetDirectory                
            let! projectsFiles =
                dirInfo |> FileSystem.TryReadFiles supportedDotNetFilesPattern            
            let projects, errors =
                projectsFiles
                |> Seq.map initProject
                |> Seq.toArray
                |> Array.partition (function
                    | Ok _ -> true
                    | Error _ -> false)                
            let projectsAndXElements = projects |> Array.choose (function Ok x -> Some x | _ -> None)
            let errors = errors |> Array.choose (function Error x -> Some x | _ -> None)
               
            let projects = projectsAndXElements |> Array.map fst
            let workspaceVersion = projectsAndXElements |> Array.map fst |> WorkspaceVersion.create
            match workspaceVersion.PropertyGroup with
            | None ->
                return! NoPropertyGroupWorkspaceVersion
                        |> Api
                        |> Error
            | Some currentVersion ->
                let timeStamp = Clock.NowUtc
                let nextVersion = Version.tryBump currentVersion timeStamp
                match nextVersion with
                | None ->
                    return! NoPropertyGroupNextVersion |> Api |> Error
                | Some nextVersion ->
                    let projectsAndXElements =
                        projectsAndXElements
                        |> Array.map (fun (project, xml) -> saveProject (project, xml) nextVersion)
                    let errors = 
                        errors |> Array.append (projectsAndXElements |> Array.choose (function Error x -> Some x | _ -> None))                    
                    let projects =
                        projectsAndXElements
                        |> Array.choose (function Ok x -> Some x | _ -> None)
                        |> Array.map fst
                    
                    return! Workspace.create (dirInfo, projects) |> Ok
        }