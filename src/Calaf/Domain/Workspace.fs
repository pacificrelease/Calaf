module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents
open Calaf.Domain.Repository
open Calaf.Domain.Suite

let tryCreate (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) =
    result {
        let suite, suiteEvents =
            directory.Projects
            |> Array.map Project.tryCreate
            |> Array.choose id
            |> create
            
        let! repoResult =
            repoInfo
            |> Option.traverseResult tryCreate
            
        let events = match repoResult with | Some (_, repoEvents) -> suiteEvents @ repoEvents | None -> suiteEvents
        
        let workspace = {
            Directory  = directory.Directory
            Repository = repoResult  |> Option.map fst 
            Suite      = suite
        }
        
        let workspaceEvent =
            WorkspaceCreated {
                Directory = directory.Directory
                RepositoryExist = repoInfo.IsSome
                RepositoryVersion = repoResult |> Option.bind (fun (repo, _) -> Repository.tryGetCalendarVersion repo)
                SuiteVersion = tryGetCalendarVersion suite
            }
            |> DomainEvent.Workspace
        let events = workspaceEvent :: events
        return workspace, events        
    }