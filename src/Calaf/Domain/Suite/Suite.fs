module internal Calaf.Domain.Suite

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
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
            
    let toSuiteBumped suite previousVersion bumpedProjects =
        match suite with        
        | StandardSet sm ->
            SuiteBumped {
                PreviousCalendarVersion = previousVersion
                NewCalendarVersion = sm.Version
                ProjectsBumpedCount = bumpedProjects |> Seq.length |> uint16
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
        | StandardSet { Version = version; Projects = projects } ->
            let bump project =
                match project with
                | Versioned { Version = CalVer _ } as Versioned p ->
                    tryBump p nextVersion
                    |> Result.map (fun p -> let p = Versioned p in (Some p, p))
                | otherProject ->
                    Ok (None, otherProject)
            
            let! bumpResults = projects |> Array.traverseResultM bump                
            let bumpedProjects, suiteProjects =
                bumpResults
                |> Array.unzip
                |> fun (bumpedProjectsOptions, allSuiteProjects) -> (Array.choose id bumpedProjectsOptions, allSuiteProjects)
                
            let updatedSuite = StandardSet { Version  = nextVersion; Projects = suiteProjects }
            let event = Events.toSuiteBumped updatedSuite version bumpedProjects            
            return (updatedSuite , [event])
    }