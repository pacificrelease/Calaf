module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities
open Calaf.Domain.DomainEvents

module Events =
    let toWorkspaceCaptured (workspace: Workspace) =
        WorkspaceCaptured {
            Directory = workspace.Directory
            Version = workspace.Version
            RepositoryExist = workspace.Repository |> Option.isSome
            RepositoryVersion = workspace.Repository |> Option.bind Repository.tryGetCalendarVersion
            SuiteVersion = Suite.getCalendarVersion workspace.Suite
        } |> DomainEvent.Workspace
        
    let toWorkspaceReleased (workspace: Workspace) previousVersion =
        WorkspaceReleased {
            Directory = workspace.Directory
            PreviousCalendarVersion = previousVersion
            NewCalendarVersion = workspace.Version
            RepositoryExist = workspace.Repository |> Option.isSome
        } |> DomainEvent.Workspace
        
let private getNextNightlyVersion (version: CalendarVersion) (dayOfMonth: DayOfMonth, monthStamp: MonthStamp) =
    Version.nightly version (dayOfMonth, monthStamp)

let private getNextReleaseVersion (version: CalendarVersion) (monthStamp: MonthStamp) =
    Version.release version monthStamp 
    
let private combineVersions suite repoOption =
    [
        yield Suite.getCalendarVersion suite
        match repoOption |> Option.bind Repository.tryGetCalendarVersion with
        | Some version -> yield version
        | None -> ()
    ]
    
let private combineEvents primaryEvents secondaryEventsOption =
    match secondaryEventsOption with
    | Some secondaryEvents ->
        primaryEvents @ secondaryEvents
    | None ->
        primaryEvents

let tryCapture (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) =
    result {
        let! suite, suiteEvents =
            directory.Projects
            |> List.map Project.tryCapture
            |> List.choose id
            |> Suite.tryCapture
            
        let! repoResult = repoInfo |> Option.traverseResult Repository.tryCapture        
        let events = match repoResult with | Some (_, repoEvents) -> suiteEvents @ repoEvents | None -> suiteEvents
        let maybeRepo = repoResult |> Option.map fst
        let! version = combineVersions suite maybeRepo |> Version.tryMax |> Option.toResult CalendarVersionMissing 
        
        let workspace = {
            Directory  = directory.Directory
            Version    = version
            Repository = maybeRepo
            Suite      = suite
        }
        
        let event = Events.toWorkspaceCaptured workspace
        let events = event :: events
        return workspace, events        
    }
    
let profile (workspace: Workspace) =
    let projectsProfiles  = Suite.tryProfile workspace.Suite
    let repositoryProfile = workspace.Repository |> Option.bind(fun p -> Repository.tryProfile p (projectsProfiles |> List.map _.AbsolutePath))
    { Projects = projectsProfiles
      Repository = repositoryProfile }
    
let tryNightly (workspace: Workspace) (dayOfMonth: DayOfMonth, monthStamp: MonthStamp) =
    result {
        let nextVersion = getNextNightlyVersion workspace.Version (dayOfMonth, monthStamp)
        if workspace.Version = nextVersion
        then
            return! WorkspaceAlreadyCurrent |> Error
        else
            let! suite', suiteEvents = Suite.tryNightly workspace.Suite nextVersion
            let! repo' =
                workspace.Repository
                |> Option.traverseResult (fun repo -> Repository.tryRelease repo nextVersion)
            
            let events =
               combineEvents suiteEvents (repo' |> Option.map snd)
                
            let workspace' =
                { workspace with
                    Version = nextVersion
                    Suite = suite'
                    Repository = repo' |> Option.map fst }
            let event = Events.toWorkspaceReleased workspace' workspace.Version
            return workspace', events @ [event] 
    }
    
let tryRelease (workspace: Workspace) (monthStamp: MonthStamp) =
    result {
        let nextVersion = getNextReleaseVersion workspace.Version monthStamp
        if workspace.Version = nextVersion
        then
            return! WorkspaceAlreadyCurrent |> Error
        else
            let! suite', suiteEvents = Suite.tryRelease workspace.Suite nextVersion
            let! repo' =
                workspace.Repository
                |> Option.traverseResult (fun repo -> Repository.tryRelease repo nextVersion)
            
            let events =
               combineEvents suiteEvents (repo' |> Option.map snd)
                
            let workspace' =
                { workspace with
                    Version = nextVersion
                    Suite = suite'
                    Repository = repo' |> Option.map fst }
            let event = Events.toWorkspaceReleased workspace' workspace.Version
            return workspace', events @ [event] 
    }