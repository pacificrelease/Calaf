module internal Calaf.Domain.Solution

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents
open Calaf.Domain.Project

module Events =
    let toSolutionCaptured solution =
        match solution with        
        | Standard { Version = version; Projects = projects } ->
            { CalendarVersion = version
              CalendarVersionProjectsCount = projects |> Seq.length |> uint16
              TotalProjectsCount = projects |> Seq.length |> uint16 }
            |> SolutionEvent.StateCaptured
            |> DomainEvent.Solution
            
    let toCollectionReleased solution previousVersion releasedProjects =
        match solution with        
        | Standard { Version = version; Projects = projects } ->
            { PreviousCalendarVersion = previousVersion
              NewCalendarVersion = version
              ProjectsBumpedCount = uint16 <| Seq.length releasedProjects
              TotalProjectsCount = uint16 <| Seq.length projects }
            |> SolutionEvent.ReleaseCreated
            |> DomainEvent.Solution
        
let tryCapture
    (projects: Project list)
    (changelog: Changelog)=
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
            let solution = Solution.Standard { Version = version; Changelog = changelog; Projects = projects }
            let event = solution |> Events.toSolutionCaptured
            return (solution, [ event ])        
    }    
        
let getCalendarVersion solution =
    match solution with
    | Standard { Version = version } -> version
    
let trySnapshot solution =
    match solution with
    | Standard { Projects = projects } ->
        projects
        |> Seq.map trySnapshot
        |> Seq.choose id
        |> Seq.toList
        
let tryRelease (solution: Solution) (nextVersion: CalendarVersion) =
    result {
        match solution with
        | Standard { Version = version; Changelog = changelog; Projects = projects } ->            
            let! projects' = projects |> List.traverseResultM (fun p -> tryRelease p nextVersion)                
            let collection' = Standard { Version = nextVersion; Changelog = changelog; Projects = projects' }
            let event  = Events.toCollectionReleased collection' version projects'            
            return (collection', [ event ])
    }