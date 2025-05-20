// Orchestrator
// TODO: Replace to Use-Cases
namespace Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Domain
open Calaf.Infrastructure

module Workspace =    
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
    [<Literal>]
    let private tenTags = 10
    
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path
        
    // TODO: Return Info/Report with Workspace&Errors for reporting
    // WorkspaceResponse/WorspaceResult
    // Use case        
    let getWorkspace
        (readDirectory: string -> string -> Result<Calaf.Contracts.DirectoryInfo, InfrastructureError>)
        (readGit: string -> int -> System.DateTimeOffset -> Result<Calaf.Contracts.GitRepositoryInfo option, InfrastructureError>)
        (path: string)
        (timeStamp: System.DateTimeOffset)
        : Result<Calaf.Domain.DomainTypes.Entities.Workspace, CalafError> =
        result {
            let path = getPathOrCurrentDir path
            let! dir = readDirectory path supportedFilesPattern |> Result.mapError CalafError.Infrastructure            
            let! repo = readGit path tenTags timeStamp          |> Result.mapError CalafError.Infrastructure            
            let! workspace, _ = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
                
            return workspace
        }
        
    let bumpWorkspace
        (readDirectory: string -> string -> Result<Calaf.Contracts.DirectoryInfo, InfrastructureError>)
        (readGit: string -> int -> System.DateTimeOffset -> Result<Calaf.Contracts.GitRepositoryInfo option, InfrastructureError>)
        (clock: unit -> System.DateTimeOffset)
        (path: string)        
        : Result<Calaf.Domain.DomainTypes.Entities.Workspace, CalafError> =
        result {
            let timeStamp = clock()
            let path = getPathOrCurrentDir path
            let! monthStamp = DateSteward.tryCreate timeStamp.DateTime |> Result.mapError CalafError.Domain
            let! dir = readDirectory path supportedFilesPattern |> Result.mapError CalafError.Infrastructure
            let! repo = readGit path tenTags timeStamp          |> Result.mapError CalafError.Infrastructure
            let! workspace, createEvents = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain           
            
            let! bumpedWorkspace, bumpEvents = Workspace.tryBump workspace monthStamp |> Result.mapError CalafError.Domain
                
            return bumpedWorkspace
        }