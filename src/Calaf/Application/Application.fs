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
    
    //val readDirectory: string -> string -> Result<Calaf.Contracts.DirectoryInfo, InfrastructureError>
        
    // TODO: Return Info/Report with Workspace&Errors for reporting
    // WorkspaceResponse/WorspaceResult
    // Use case        
    let getWorkspace
        (readDirectory: string -> string -> Result<Calaf.Contracts.DirectoryInfo, InfrastructureError>)
        (readGit: string -> int -> Result<Calaf.Contracts.GitRepositoryInfo option, InfrastructureError>)
        (path: string)
        : Result<Calaf.Domain.DomainTypes.Workspace, CalafError> =
        result {
            let! dir = readDirectory path supportedFilesPattern |> Result.mapError CalafError.Infrastructure
            let! repo = readGit path tenTags                    |> Result.mapError CalafError.Infrastructure
            
            let workspace =
                Workspace.create (dir, repo)
                
            return workspace            
        }
