module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities
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
        
    let toWorkspaceBumped (workspace: Workspace) previousVersion =
        WorkspaceBumped {
            Directory = workspace.Directory
            PreviousCalendarVersion = previousVersion
            NewCalendarVersion = workspace.Version
            RepositoryExist = workspace.Repository |> Option.isSome
        } |> DomainEvent.Workspace

let private getNextVersion (workspace: Workspace) (monthStamp: MonthStamp) =
    Version.bump workspace.Version monthStamp
    
let private combineVersions suite repoOption =
    [
        yield Suite.getCalendarVersion suite
        match repoOption |> Option.bind Repository.tryGetCalendarVersion with
        | Some version -> yield version
        | None -> ()
    ]
    
let private combineEventsO primaryEvents secondaryEventsOption =
    match secondaryEventsOption with
    | Some secondaryEvents ->
        primaryEvents @ secondaryEvents
    | None ->
        primaryEvents

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
    
let tryBump (workspace: Workspace) (monthStamp: MonthStamp) =
    result {
        let nextVersion = getNextVersion workspace monthStamp
        if workspace.Version = nextVersion
        then
            return! CurrentWorkspace |> Error
        else
            let! bumpedSuite, suiteEvents = Suite.tryBump workspace.Suite nextVersion
            let! bumpedRepoOption =
                workspace.Repository
                |> Option.traverseResult (fun repo -> Repository.tryBump repo nextVersion)
            
            let events =
               combineEventsO suiteEvents (bumpedRepoOption |> Option.map snd)
                
            let updatedWorkspace =
                { workspace with
                    Version = nextVersion
                    Suite = bumpedSuite
                    Repository = bumpedRepoOption |> Option.map fst }
            let event = Events.toWorkspaceBumped updatedWorkspace workspace.Version
            return updatedWorkspace, events @ [event] 
    }