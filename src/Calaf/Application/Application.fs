// Orchestrator
// TODO: Replace to Use-Cases
module Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Domain
open Calaf.Infrastructure

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
