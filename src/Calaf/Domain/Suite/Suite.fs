module internal Calaf.Domain.Suite

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Project

let bump (suite: Suite) (nextVersion: CalendarVersion) =
    result {
        match suite.Version with
        | Some _ ->
            let! bumpedProjects =
                suite.Projects
                |> Array.traverseResultM (fun project -> 
                    match project with
                    | Versioned { Version = CalVer _ } as Versioned p ->
                        tryBump p nextVersion |> Result.map Versioned
                    | project -> Ok project)
                    
            return { 
                Version = Some nextVersion
                Projects = bumpedProjects 
            }
        | None ->
            return! NoProjectsVersion |> Error        
    }
    
let create (projects: Project[]) : Suite =
    { Version  = projects |> chooseCalendarVersions |> Version.tryMax
      Projects = projects }