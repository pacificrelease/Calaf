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
        asyncResult {
            let! bumpedProject, xml = Project.tryBump xml project newVersion
            match bumpedProject with
            | Bumped (metadata, lang, prevVer, curVer) ->
                return! Xml.TrySaveXml metadata.AbsolutePath xml            
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
                    return! Workspace.create (dirInfo, projects) |> Ok
        }
    
    // let CreateWorkspace workingDir : Result<Workspace, CalafError> =
    //     result {             
    //         let tryParseProject (metadata: ProjectMetadata) : Project option =
    //             option {                
    //                 let xmlResult = Xml.TryLoadXml(metadata.AbsolutePath)
    //                 match xmlResult with
    //                 | Error _ ->
    //                     // Handle error or mb add new type of the Project -> Unavailable (for example)
    //                     return! None
    //                 | Ok xml ->
    //                     return! Project.tryCreate xml metadata
    //             }
    //         // TODO: Refactor to use async parallel read projects
    //         let loadProjects (workingDir : DirectoryInfo) =
    //             result {
    //                 let! xmlFiles = FileSystem.TryReadFiles supportedDotNetFilesPattern workingDir
    //                 return xmlFiles 
    //                     |> Seq.map ProjectMetadata.create
    //                     |> Seq.choose tryParseProject
    //                     |> Seq.toArray
    //             }
    //         
    //         let! directory = workingDir |> FileSystem.TryGetDirectory |> Result.mapError CalafError.FileSystem
    //         let! projects = directory |> loadProjects |> Result.mapError CalafError.FileSystem
    //         return Workspace.create(directory, projects)
    //     }
    //     
    // let BumpWorkspace (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
    //     option {
    //         let! propertyGroup = workspace.Version.PropertyGroup
    //         let! nextPropertyGroupVersion = Version.tryBump propertyGroup timeStamp |> Some
    //         return { PropertyGroup = nextPropertyGroupVersion }
    //     }    
    //
    // // TODO: Remove this function because of another workflows is going to be used
    // let GetNextVersion (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
    //     option {
    //         let! propertyGroup = workspace.Version.PropertyGroup
    //         let nextPropertyGroupVersion = Version.tryBump propertyGroup timeStamp
    //         return { PropertyGroup = nextPropertyGroupVersion }
    //     }