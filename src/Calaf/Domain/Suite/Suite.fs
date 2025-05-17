module internal Calaf.Domain.Suite

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

let create (projects: Project[]) =
    match projects with
    | [||] ->        
        EmptyProjectsSuite |> Error
    | p ->
        let version = p |> chooseCalendarVersions |> Version.tryMax
        let suite = { Version = version; Projects = p } |> Suite.StandardSet
        let event = suite |> Events.toSuiteCreated
        (suite, [event])  |> Ok
        
let tryGetCalendarVersion suite =
    match suite with
    | StandardSet { Version = version } -> version
    
let tryBump (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite with
        | StandardSet { Version = Some _; Projects = projects } ->
            let! bumpedProjects =
                projects
                |> Array.traverseResultM (function                    
                    | Versioned { Version = CalVer _ } as Versioned p ->
                        tryBump p nextVersion |> Result.map Versioned
                    | project -> Ok project)                    
            let sm = { 
                Version = Some nextVersion
                Projects = bumpedProjects 
            }
            return sm |> StandardSet
        | StandardSet _ ->
            return! NotFoundCalendarVersionPrerequisites |> Error
    }