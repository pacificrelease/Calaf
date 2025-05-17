module internal Calaf.Domain.Suite

open Calaf.Extensions.InternalExtensions
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents
open Calaf.Domain.Project

module Events =
    let toSuiteCreated suite =
        match suite with        
        | StandardSet sm ->
            SuiteCreated {
                CalendarVersion = sm.Version
                CalendarVersionProjectsCount = chooseCalendarVersioned sm.Projects |> Seq.length |> uint16
                TotalProjectsCount = sm.Projects |> Seq.length |> uint16
            } |> DomainEvent.Suite

let tryCreate (projects: Project[]) =
    result {
        match projects with
        | [||] ->
            return! EmptyProjectsSuite |> Error            
        | projects ->
            let! version = projects
                        |> chooseCalendarVersions
                        |> Version.tryMax
                        |> Option.toResult NoCalendarVersion                
            let suite = { Version = version; Projects = projects } |> Suite.StandardSet
            let event = suite |> Events.toSuiteCreated
            return (suite, [event])        
    }    
        
let getCalendarVersion suite =
    match suite with
    | StandardSet { Version = version } -> version
    
let tryBump (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite with
        | StandardSet { Version = _; Projects = projects } ->
            let! bumpedProjects =
                projects
                |> Array.traverseResultM (function                    
                    | Versioned { Version = CalVer _ } as Versioned p ->
                        tryBump p nextVersion |> Result.map Versioned
                    | project -> Ok project)                    
            return { Version  = nextVersion; Projects = bumpedProjects } |> StandardSet
    }