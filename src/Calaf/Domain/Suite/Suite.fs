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
            
    let toSuiteBumped suite previousVersion bumpedProjects =
        match suite with        
        | StandardSet (version, projects) ->
            SuiteBumped {
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
    
let chooseXmlProjects suite =
    match suite with
    | StandardSet (_, projects) ->
        projects
        |> chooseXmlVCalendarVersionedProjects
    
let tryBump (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite with
        | StandardSet (version, projects) ->
            let bump project =
                match project with
                | Versioned { Version = CalVer _ } as Versioned p ->
                    tryBump p nextVersion
                    |> Result.map (fun p -> let p = Versioned p in (Some p, p))
                | otherProject ->
                    Ok (None, otherProject)
            
            let! bumpResults = projects |> List.traverseResultM bump                
            let bumpedProjects, suiteProjects =
                bumpResults
                |> List.unzip
                |> fun (bumpedProjectsOptions, allSuiteProjects) -> (List.choose id bumpedProjectsOptions, allSuiteProjects)
                
            let updatedSuite = StandardSet (nextVersion, suiteProjects)
            let event = Events.toSuiteBumped updatedSuite version bumpedProjects            
            return (updatedSuite , [event])
    }