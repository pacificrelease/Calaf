module internal Calaf.Domain.Suite

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents
open Calaf.Domain.Project

let toSuiteCreated suite =
    match suite with
    | Empty  ->
        SuiteCreated {
            CalendarVersion = None
            CalendarVersionProjectsCount = 0us
            TotalProjectsCount = 0us
        } |> DomainEvent.Suite
    | Set sm ->
        SuiteCreated {
            CalendarVersion = sm.Version
            CalendarVersionProjectsCount = chooseCalendarVersioned sm.Projects |> Seq.length |> uint16
            TotalProjectsCount = sm.Projects |> Seq.length |> uint16
        } |> DomainEvent.Suite
    
    
let bump (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite with
        | Set { Version = Some _; Projects = projects } ->
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
            return sm |> Set
        | Set _ ->
            return! NotFoundCalendarVersionPrerequisites |> Error
        | Empty ->
            return! BumpEmptySuite |> Error
    }
    
let create (projects: Project[]) =
    match projects with
    | [||] ->        
        let suite = Suite.Empty
        let event = toSuiteCreated suite
        suite, [event]
    | p ->
        let version = p |> chooseCalendarVersions |> Version.tryMax
        let suite = { Version = version; Projects = p } |> Suite.Set
        let event = toSuiteCreated suite
        suite, [event]