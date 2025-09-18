module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

module Events =
    let toWorkspaceCaptured (workspace: Workspace) =
        { Directory = workspace.Directory
          Version = workspace.Version
          RepositoryExist = workspace.Repository |> Option.isSome
          RepositoryVersion = workspace.Repository |> Option.bind Repository.tryGetCalendarVersion
          SolutionVersion = Solution.getCalendarVersion workspace.Solution }
        |> WorkspaceEvent.StateCaptured
        |> DomainEvent.Workspace
        
    let toWorkspaceReleased (workspace: Workspace) previousVersion =
        { Directory = workspace.Directory
          PreviousCalendarVersion = previousVersion
          NewCalendarVersion = workspace.Version
          RepositoryExist = workspace.Repository |> Option.isSome }
        |> WorkspaceEvent.ReleaseCreated
        |> DomainEvent.Workspace
    
let private combineVersions collection repoOption =
    [
        yield Solution.getCalendarVersion collection
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
        let! solution, solutionEvents =
            directory.Projects
            |> List.map Project.tryCapture
            |> List.choose id
            |> Solution.tryCapture <| Changelog.tryCapture directory.Changelog
        
        let! repoResult =
            repoInfo
            |> Option.traverseResult Repository.tryCapture        
        let events =
            match repoResult with
            | Some (_, repoEvents) ->
                solutionEvents @ repoEvents
            | None -> solutionEvents
        let maybeRepo = repoResult |> Option.map fst        
        let! version =
            combineVersions solution maybeRepo
            |> Version.tryMax
            |> Option.toResult CalendarVersionMissing                
        let workspace = {
            Directory  = directory.Directory
            Version    = version
            Repository = maybeRepo
            Solution   = solution
        }
        
        let event = Events.toWorkspaceCaptured workspace
        let events = event :: events
        return workspace, events     
    }
    
let snapshot (workspace: Workspace) (changeset: Changeset option) =
    let projectsSnapshot  = Solution.trySnapshot workspace.Solution
    let repositorySnapshot =
        workspace.Repository
        |> Option.bind (fun repo ->            
            Repository.trySnapshot repo (projectsSnapshot |> List.map _.AbsolutePath))
    let changelogSnapshot =
        changeset
        |> Option.map (Changelog.snapshot workspace)
    { Projects = projectsSnapshot
      Changelog = changelogSnapshot
      Repository = repositorySnapshot }

let tryRelease (workspace: Workspace) (nextVersion: CalendarVersion) =
    result {
        let sameVersion =
            workspace.Version
            |> (=) nextVersion
        if sameVersion
        then
            return! WorkspaceAlreadyCurrent |> Error
        else
            let! solution', solutionEvents =
                Solution.tryRelease workspace.Solution nextVersion            
            let! repo' =
                match workspace.Repository with
                | Some repo ->                    
                    let r = Repository.tryRelease repo nextVersion
                    r |> Result.map Some
                | None ->
                    Ok None
            
            let events =
               combineEvents solutionEvents (repo' |> Option.map snd)
                
            let workspace' =
                { workspace with
                    Version = nextVersion
                    Solution = solution'
                    Repository = repo' |> Option.map fst }
            let event = Events.toWorkspaceReleased workspace' workspace.Version
            return workspace', events @ [event] 
    }