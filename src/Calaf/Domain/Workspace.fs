module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

module Events =
    let toWorkspaceCreated (workspace: Workspace) =
        WorkspaceCreated {
            Directory = workspace.Directory
            Version = workspace.Version
            RepositoryExist = workspace.Repository |> Option.isSome
            RepositoryVersion = workspace.Repository |> Option.bind Repository.tryGetCalendarVersion
            SuiteVersion = Suite.getCalendarVersion workspace.Suite
        } |> DomainEvent.Workspace    

let private combineVersions suite repoOption =
    [
        yield Suite.getCalendarVersion suite
        match repoOption |> Option.bind Repository.tryGetCalendarVersion with
        | Some version -> yield version
        | None -> ()
    ]

let tryCreate (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) =
    result {
        let! suite, suiteEvents =
            directory.Projects
            |> Array.map Project.tryCreate
            |> Array.choose id
            |> Suite.tryCreate
            
        let! repoResult = repoInfo |> Option.traverseResult Repository.tryCreate        
        let events = match repoResult with | Some (_, repoEvents) -> suiteEvents @ repoEvents | None -> suiteEvents
        let maybeRepo = repoResult |> Option.map fst
        let! version = combineVersions suite maybeRepo |> Version.tryMax |> Option.toResult NoCalendarVersion 
        
        let workspace = {
            Directory  = directory.Directory
            Version    = version
            Repository = maybeRepo
            Suite      = suite
        }
        
        let event = Events.toWorkspaceCreated workspace
        let events = event :: events
        return workspace, events        
    }