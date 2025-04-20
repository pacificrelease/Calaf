module internal Calaf.Domain.Project
    
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors

module internal Schema =
    
    [<Literal>]
    let private VersionXElementName = "Version"
    [<Literal>]
    let private PropertyGroupXElementName = "PropertyGroup"    
    
    // TODO: Use ERROR instead of option?
    let tryExtractVersionElement (xElement: System.Xml.Linq.XElement) =
        xElement.Elements(PropertyGroupXElementName)
        |> Seq.map _.Elements(VersionXElementName)
        |> Seq.tryExactlyOne
        |> Option.bind Seq.tryHead
        
    let setVersion (version: string) (xElement: System.Xml.Linq.XElement) =
        xElement.Value <- version
        xElement
    
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
    
let tryCreate (xml: System.Xml.Linq.XElement) (metadata: ProjectMetadata) : Project option =        
    let tryExtractVersion (xml: System.Xml.Linq.XElement) : Version option =
        xml
        |> Schema.tryExtractVersionElement
        |> Option.bind (fun x -> Version.tryParse <| x.Value)
        
    option {            
        let! language = Language.tryParse metadata.Extension
        let version = xml |> tryExtractVersion
        return
            match version with
            | Some version -> Versioned(metadata, language, version)
            | None         -> Unversioned(metadata, language)
    }
    
let tryBump (xml: System.Xml.Linq.XElement) (project: Project) (nextVersion: CalendarVersion) =
    let tryInsertVersion (nextVersion: CalendarVersion) (xml: System.Xml.Linq.XElement) : System.Xml.Linq.XElement option =        
        xml
        |> Schema.tryExtractVersionElement
        |> Option.map (fun x -> Schema.setVersion (nextVersion |> Version.toString ) x)
        
    match project with
    | Versioned (projectMetadata, lang, CalVer currentVersion) ->
        (projectMetadata, lang, currentVersion, nextVersion)
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
        