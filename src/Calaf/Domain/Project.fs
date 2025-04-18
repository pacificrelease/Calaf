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
    // TODO: Use ERROR instead of option?
    let tryExtractVersionElements(xElement: System.Xml.Linq.XElement) =
        xElement.Elements("PropertyGroup")
        |> Seq.map _.Elements("Version")
        |> Seq.tryExactlyOne
        
    let tryExtractVersionString (versionElements: System.Xml.Linq.XElement seq) : string option =
        match versionElements with
        | seq when Seq.isEmpty seq -> None
        | seq when Seq.length seq > 1 -> None
        | seq -> Some (seq |> Seq.head).Value
        
    let tryExtractVersion (xml: System.Xml.Linq.XElement) : Version option =
        option {
            let! versionsElements = xml |> tryExtractVersionElements
            let! bareVersion = versionsElements |> tryExtractVersionString  
            return! bareVersion |> Version.tryParse
        }
        
    option {            
        let! language = Language.tryParse metadata.Extension
        let version = xml |> tryExtractVersion
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
    | Versioned _ ->
        NoCalendarVersionProject
        |> Error
    | Unversioned _ ->
        UnversionedProject
        |> Error
    | Bumped _ ->
        AlreadyBumpedProject
        |> Error 