// Orchestrator
// TODO: Replace to Use-Cases
namespace Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Infrastructure

module Project = 
    let bump (project, xml) newVersion =        
        result {            
            let! bumped, xml = Project.tryBump xml project newVersion
            return (bumped, xml)
        }
    
    // There are no errors in the composition root
    let save (project, xml) =
        result {
            match project with
            | Bumped (metadata, _, _, _) ->
                let! xml = Xml.trySaveXml metadata.AbsolutePath xml |> Result.mapError Infrastructure
                return (project, xml)
            | Versioned (metadata, _, _) ->
                return! GivenNotBumpedProject metadata.Name
                    |> Domain
                    |> Error
            | Skipped (metadata, _, _) ->
                return! GivenSkippedProject metadata.Name
                        |> Domain
                        |> Error
            | Unversioned (metadata, _) ->
                return! GivenUnversionedProject metadata.Name                
                        |> Domain
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
        
    // Use case
    let getWorkspace dir =
        result {
            let! directoryInfo = Calaf.Infrastructure.FileSystem.tryReadWorkspace dir supportedFilesPattern
            let! repoInfo = Git.tryReadRepository directoryInfo.Directory hundredTags
            // TODO: Return Info/Report with Workspace&Errors for reporting
            // WorkspaceResponse/WorspaceResult
            return Workspace.create (directoryInfo, repoInfo)
        }