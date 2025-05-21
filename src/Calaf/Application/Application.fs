// Orchestrator
// TODO: Replace to Use-Cases
namespace Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Domain

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
        (path: string)
        (git: IGit)
        (fileSystem: IFileSystem)
        (clock: IClock)        
        : Result<Calaf.Domain.DomainTypes.Entities.Workspace, CalafError> =
        result {
            let path = getPathOrCurrentDir path
            let! dir = fileSystem.tryReadDirectory path supportedFilesPattern
            let timeStamp = clock.now()
            let! repo = git.tryRead path tenTags Version.versionPrefixes timeStamp 
            let! workspace, _ = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
                
            return workspace
        }
        
    let bumpWorkspace
        (path: string)
        (git: IGit)
        (fileSystem: IFileSystem)
        (clock: IClock)        
        : Result<Calaf.Domain.DomainTypes.Entities.Workspace, CalafError> =
        result {
            let path = getPathOrCurrentDir path
            let timeStamp = clock.now()            
            let! monthStamp = DateSteward.tryCreate timeStamp.DateTime |> Result.mapError CalafError.Domain
            let! dir = fileSystem.tryReadDirectory path supportedFilesPattern
            let! repo = git.tryRead path tenTags Version.versionPrefixes timeStamp
            let! workspace, createEvents = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain           
            
            let! bumpedWorkspace, bumpEvents = Workspace.tryBump workspace monthStamp |> Result.mapError CalafError.Domain
                
            return bumpedWorkspace
        }