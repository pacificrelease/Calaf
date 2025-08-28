module internal Calaf.Domain.Collection

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents
open Calaf.Domain.Project

module Events =
    let toCollectionCaptured collection =
        match collection with        
        | Standard (version, projects) ->
            { CalendarVersion = version
              CalendarVersionProjectsCount = chooseCalendarVersioned projects |> Seq.length |> uint16
              TotalProjectsCount = projects |> Seq.length |> uint16 }
            |> CollectionEvent.StateCaptured
            |> DomainEvent.Collection
            
    let toCollectionReleased collection previousVersion bumpedProjects =
        match collection with        
        | Standard (version, projects) ->
            { PreviousCalendarVersion = previousVersion
              NewCalendarVersion = version
              ProjectsBumpedCount = bumpedProjects |> Seq.length |> uint16
              TotalProjectsCount = projects |> Seq.length |> uint16 }
            |> CollectionEvent.ReleaseCreated
            |> DomainEvent.Collection
            
let private chooseCalendarVersionedProjects collection _ =
    match collection with
    | Standard (_, projects) ->
        projects
        |> chooseCalendarVersioned
        |> Seq.toList
        
let tryCapture (projects: Project list) =
    result {
        match projects with
        | projects when Seq.isEmpty projects ->
            return! ProjectCollectionEmpty |> Error            
        | projects ->
            let! version =
                projects
                |> chooseCalendarVersions
                |> Version.tryMax
                |> Option.toResult CalendarVersionMissing                
            let collection = (version, projects) |> Collection.Standard
            let event = collection |> Events.toCollectionCaptured
            return (collection, [ event ])        
    }    
        
let getCalendarVersion collection =
    match collection with
    | Standard (version, _) -> version
    
let tryProfile collection =
    match collection with
    | Standard (_, projects) ->
        projects
        |> Seq.map tryProfile
        |> Seq.choose id
        |> Seq.toList
        
let tryRelease (collection: Collection) (nextVersion: CalendarVersion) =
    result {
        match collection with
        | Standard (version, projects) ->
            let nightly project =
                match project with
                | Versioned { Version = CalVer _ } as Versioned p ->
                    tryRelease p nextVersion
                    |> Result.map (fun p -> let p = Versioned p in (Some p, p))
                | otherProject ->
                    Ok (None, otherProject)
            
            let! result = projects |> List.traverseResultM nightly
            let nightlyProjects, collectionProjects =
                result
                |> List.unzip
                |> fun (releasedCollectionProjects, allCollectionProjects) ->
                    (List.choose id releasedCollectionProjects, allCollectionProjects)
                
            let collection' = Standard (nextVersion, collectionProjects)
            let event  = Events.toCollectionReleased collection' version nightlyProjects            
            return (collection' , [ event ])
    }