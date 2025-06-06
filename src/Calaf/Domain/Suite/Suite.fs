module internal Calaf.Domain.Suite

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities
open Calaf.Domain.DomainEvents
open Calaf.Domain.Project

module Events =
    let toSuiteCaptured suite =
        match suite with        
        | StandardSet (version, projects) ->
            SuiteCaptured {
                CalendarVersion = version
                CalendarVersionProjectsCount = chooseCalendarVersioned projects |> Seq.length |> uint16
                TotalProjectsCount = projects |> Seq.length |> uint16
            } |> DomainEvent.Suite
            
    let toSuiteReleased suite previousVersion bumpedProjects =
        match suite with        
        | StandardSet (version, projects) ->
            SuiteReleased {
                PreviousCalendarVersion = previousVersion
                NewCalendarVersion = version
                ProjectsBumpedCount = bumpedProjects |> Seq.length |> uint16
                TotalProjectsCount = projects |> Seq.length |> uint16
            } |> DomainEvent.Suite
            
let private chooseCalendarVersionedProjects suite =
    match suite with
    | StandardSet (_, projects) ->
        projects
        |> chooseCalendarVersioned
        |> Seq.toList
        
let tryCapture (projects: Project list) =
    result {
        match projects with
        | projects when Seq.isEmpty projects ->
            return! ProjectSuiteEmpty |> Error            
        | projects ->
            let! version = projects
                        |> chooseCalendarVersions
                        |> Version.tryMax
                        |> Option.toResult CalendarVersionMissing                
            let suite = (version, projects) |> Suite.StandardSet
            let event = suite |> Events.toSuiteCaptured
            return (suite, [event])        
    }    
        
let getCalendarVersion suite =
    match suite with
    | StandardSet (version, _) -> version
    
let tryProfile suite =
    match suite with
    | StandardSet (_, projects) ->
        projects |> Seq.map tryProfile |> Seq.choose id |> Seq.toList        
    
let tryRelease (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite with
        | StandardSet (version, projects) ->
            let release project =
                match project with
                | Versioned { Version = CalVer _ } as Versioned p ->
                    tryRelease p nextVersion
                    |> Result.map (fun p -> let p = Versioned p in (Some p, p))
                | otherProject ->
                    Ok (None, otherProject)
            
            let! result = projects |> List.traverseResultM release
            let releasedProjects, suiteProjects =
                result
                |> List.unzip
                |> fun (releasedSuiteProjects, allSuiteProjects) -> (List.choose id releasedSuiteProjects, allSuiteProjects)
                
            let suite' = StandardSet (nextVersion, suiteProjects)
            let event  = Events.toSuiteReleased suite' version releasedProjects            
            return (suite' , [event])
    }