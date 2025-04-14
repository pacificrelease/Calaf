module internal Calaf.Domain.Project
    
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors

let choosePending (projects: Project[]) : Project[] =
    projects
    |> Array.choose (function
        | Versioned(_, _, CalVer _) as project -> Some project
        | _                                    -> None)

let chooseCalendarVersions (projects: Project[]) : CalendarVersion[] =
    projects
    |> Array.choose (function
        | Versioned (_, _, CalVer version) -> Some version
        | Bumped (_, _, _, version)        -> Some version
        | _                                -> None)    
    
let tryCreate (metadata: ProjectMetadata, xml: System.Xml.Linq.XElement) : Project option =
    option {            
        let! language = Language.tryParse metadata.Extension
        let version = Version.tryParse xml
        return
            match version with
            | Some version -> Versioned(metadata, language, version)
            | None         -> Unversioned(metadata, language)
    }
    
let tryBump (project: Project, nextVersion: CalendarVersion) =
    match project with
    | Versioned(pm, lang, CalVer currentVersion) ->
        (pm, lang, currentVersion, nextVersion)
        |> Bumped
        |> Ok
    | Versioned _ as v ->
        v
        |> InvalidProjectVersionError 
        |> Error
    | Unversioned _ as u ->
        u
        |> UnversionedProjectError
        |> Error
    | Bumped _ as b ->
        b
        |> BumpedProjectError
        |> Error 