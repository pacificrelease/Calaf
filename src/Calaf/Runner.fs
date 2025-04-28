// Api. Composition Root
namespace Calaf

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.Errors
open Calaf.Domain
open Calaf.Api.Project

module Runner =
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
        
    let initWorkspace dir =                
        result {
            let! dir     = dir |> FileSystem.tryGetDirectory
            let! repo = dir |> Git.tryReadRepository
            let! files   = dir |> FileSystem.tryReadFiles supportedFilesPattern
            
            let lPprojects,
                lErrors = files
                |> Seq.map load
                |> Result.partition
            
            let! currentVer =
                lPprojects
                |> Seq.map fst
                |> WorkspaceVersion.create
                |> fun x -> x.PropertyGroup |> Option.toResult (NoPropertyGroupWorkspaceVersion |> Api)                                
            let timeStamp = Clock.now()
            let! bumpedVer =
                Version.tryBump currentVer timeStamp.UtcDateTime
                |> Option.toResult (NoPropertyGroupNextVersion |> Api)
            let bProjects,
                bErrors = lPprojects
                |> Project.choosePending
                |> Seq.map (fun pair -> bump pair bumpedVer)
                |> Result.partition
                            
            let sProjects,
                sErrors =  bProjects
                |> Seq.map save
                |> Result.partition

            return Workspace.create (dir, sProjects |> Seq.map fst)
        }