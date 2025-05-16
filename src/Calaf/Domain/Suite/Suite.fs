module internal Calaf.Domain.Suite

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Project

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
    | [||] -> Empty
    | _ ->
        let setCalendarVersionVersion = projects |> chooseCalendarVersions |> Version.tryMax        
        { Version  = setCalendarVersionVersion; Projects = projects } |> Set