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
          CollectionVersion = Collection.getCalendarVersion workspace.Collection }
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
        yield Collection.getCalendarVersion collection
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
        let! collection, collectionEvents =
            directory.Projects
            |> List.map Project.tryCapture
            |> List.choose id
            |> Collection.tryCapture
            
        let! repoResult =
            repoInfo
            |> Option.traverseResult Repository.tryCapture        
        let events =
            match repoResult with
            | Some (_, repoEvents) ->
                collectionEvents @ repoEvents
            | None -> collectionEvents
        let maybeRepo = repoResult |> Option.map fst
        let! version =
            combineVersions collection maybeRepo
            |> Version.tryMax
            |> Option.toResult CalendarVersionMissing 
        
        let workspace = {
            Directory  = directory.Directory
            Version    = version
            Repository = maybeRepo
            Collection = collection
        }
        
        let event = Events.toWorkspaceCaptured workspace
        let events = event :: events
        return workspace, events     
    }
    
let profile (workspace: Workspace) =
    let projectsProfiles  = Collection.tryProfile workspace.Collection
    let repositoryProfile =
        workspace.Repository
        |> Option.bind (fun p ->
            Repository.tryProfile p (projectsProfiles |> List.map _.AbsolutePath))
    { Projects = projectsProfiles
      Repository = repositoryProfile }

let tryRelease (workspace: Workspace) (nextVersion: CalendarVersion) =
    result {
        let sameVersion =
            workspace.Version
            |> (=) nextVersion
        if sameVersion
        then
            return! WorkspaceAlreadyCurrent |> Error
        else
            let! collection', collectionEvents = Collection.tryRelease workspace.Collection nextVersion
            
            let! repo' =
                match workspace.Repository with
                | Some repo ->                    
                    let r = Repository.tryRelease repo nextVersion
                    r |> Result.map Some
                | None ->
                    Ok None
            
            let events =
               combineEvents collectionEvents (repo' |> Option.map snd)
                
            let workspace' =
                { workspace with
                    Version = nextVersion
                    Collection = collection'
                    Repository = repo' |> Option.map fst }
            let event = Events.toWorkspaceReleased workspace' workspace.Version
            return workspace', events @ [event] 
    }