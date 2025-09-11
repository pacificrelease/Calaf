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
              CalendarVersionProjectsCount = projects |> Seq.length |> uint16
              TotalProjectsCount = projects |> Seq.length |> uint16 }
            |> CollectionEvent.StateCaptured
            |> DomainEvent.Collection
            
    let toCollectionReleased collection previousVersion releasedProjects =
        match collection with        
        | Standard (version, projects) ->
            { PreviousCalendarVersion = previousVersion
              NewCalendarVersion = version
              ProjectsBumpedCount = uint16 <| Seq.length releasedProjects
              TotalProjectsCount = uint16 <| Seq.length projects }
            |> CollectionEvent.ReleaseCreated
            |> DomainEvent.Collection
        
let tryCapture (projects: Project list) =
    result {
        match projects with
        | projects when Seq.isEmpty projects ->
            return! ProjectCollectionEmpty |> Error            
        | projects ->
            let projects =
                projects
                |> chooseCalendarVersionVersionedProjects
                |> Seq.toList
            let! version =
                projects
                |> chooseCalendarVersions
                |> Version.tryMax
                |> Option.toResult CalendarVersionProjectsEmpty                
            let collection = (version, projects) |> Collection.Standard
            let event = collection |> Events.toCollectionCaptured
            return (collection, [ event ])        
    }    
        
let getCalendarVersion collection =
    match collection with
    | Standard (version, _) -> version
    
let trySnapshot collection =
    match collection with
    | Standard (_, projects) ->
        projects
        |> Seq.map trySnapshot
        |> Seq.choose id
        |> Seq.toList
        
let tryRelease (collection: Collection) (nextVersion: CalendarVersion) =
    result {
        match collection with
        | Standard (version, projects) ->            
            let! projects' = projects |> List.traverseResultM (fun p -> tryRelease p nextVersion)                
            let collection' = Standard (nextVersion, projects')
            let event  = Events.toCollectionReleased collection' version projects'            
            return (collection' , [ event ])
    }