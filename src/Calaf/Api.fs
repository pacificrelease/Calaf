// Composition Root
namespace Calaf.Api

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain
open Calaf.Infrastructure

module Project =    
    let load projectFileInfo =
        let createProject metadata xml =            
            match Project.tryCreate xml metadata with
            | None -> CannotCreateProject metadata.Name |> Init |> Error
            | Some project -> (project, xml) |> Ok
            
        result {
            let metadata = ProjectMetadata.create projectFileInfo            
            let! xml = Xml.tryLoadXml(metadata.AbsolutePath)            
            return! createProject metadata xml
        }
        
    let bump (project, xml) newVersion =        
        result {            
            let! bumped, xml = Project.tryBump xml project newVersion
            return (bumped, xml)
        }
        
    let save (project, xml) =
        result {
            match project with
            | Bumped (metadata, _, _, _) ->
                let! xml = Xml.trySaveXml metadata.AbsolutePath xml
                return (project, xml)
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

// let initWorkspace dir =                
//     result {
//         let! dir     = dir |> FileSystem.tryGetDirectory
//         let! repo = dir |> Git.tryReadRepository
//         let! files   = dir |> FileSystem.tryReadFiles supportedFilesPattern
//         
//         let repo = Api.Git.init repo
//         let lPprojects,
//             lErrors = files
//             |> Seq.map Api.Project.load
//             |> Result.partition
//         
//         let! currentVer =
//             lPprojects
//             |> Seq.map fst
//             |> WorkspaceVersion.create
//             |> fun x -> x.PropertyGroup |> Option.toResult (NoPropertyGroupWorkspaceVersion |> Api)                                
//         let timeStamp = Clock.now()
//         let! bumpedVer =
//             Version.tryBump currentVer timeStamp.UtcDateTime
//             |> Option.toResult (NoPropertyGroupNextVersion |> Api)
//         let bProjects,
//             bErrors = lPprojects
//             |> Project.choosePending
//             |> Seq.map (fun pair -> Api.Project.bump pair bumpedVer)
//             |> Result.partition
//                         
//         let sProjects,
//             sErrors =  bProjects
//             |> Seq.map Api.Project.save
//             |> Result.partition
//
//         return Workspace.create (dir, sProjects |> Seq.map fst)
//     }

module Workspace =
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
    [<Literal>]
    let private hundredTags = 100
        
    let create dir =
        result {
            let! dir   = FileSystem.tryGetDirectory dir
            let! repo  = Git.tryReadRepository dir hundredTags
            let! files = FileSystem.tryReadFiles dir supportedFilesPattern 
            
            let projects, _ =
                files
                |> Seq.map Project.load
                |> Result.partition
            
            // TODO: Return Info/Report with Workspace&Errors for reporting
            // WorkspaceResponse/WorspaceResult
            return Workspace.create (dir, repo, projects |> Seq.map fst)
        }