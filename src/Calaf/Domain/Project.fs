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
    let tryExtractVersionElement (projectDocument: System.Xml.Linq.XElement) =
        projectDocument.Elements(PropertyGroupXElementName)
        |> Seq.map _.Elements(VersionXElementName)
        |> Seq.tryExactlyOne
        |> Option.bind Seq.tryHead
        
    let tryUpdateVersionElement (projectDocument: System.Xml.Linq.XElement) (version: string)=
        option {
            // TODO: Maybe better to divide variables, then update and return projectDocument
            return! projectDocument.Elements(PropertyGroupXElementName)
            |> Seq.map _.Elements(VersionXElementName)
            |> Seq.tryExactlyOne
            |> Option.bind Seq.tryHead
            |> Option.map (fun versionElement -> versionElement.SetValue(version); projectDocument)
        }
    
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
    
let tryCreate (projectDocument: System.Xml.Linq.XElement) (metadata: ProjectMetadata) : Project option =        
    let tryExtractVersion (xml: System.Xml.Linq.XElement) : Version option =
        xml
        |> Schema.tryExtractVersionElement
        |> Option.bind (fun x -> Version.tryParse <| x.Value)
        
    option {            
        let! language = Language.tryParse metadata.Extension
        let version = projectDocument |> tryExtractVersion
        return
            match version with
            | Some version -> Versioned(metadata, language, version)
            | None         -> Unversioned(metadata, language)
    }
    
let tryBump (projectDocument: System.Xml.Linq.XElement) (project: Project) (nextVersion: CalendarVersion) =    
    let tryUpdateVersionElement (projectMetadata: ProjectMetadata) =        
        match Schema.tryUpdateVersionElement projectDocument (Version.toString nextVersion)  with
        | Some updated -> updated |> Ok
        | None -> projectMetadata.Name |> CannotUpdateVersionElement |> Error
    
    match project with
    | Versioned (projectMetadata, lang, CalVer currentVersion) ->        
        result {
            let! updatedProjectDocument = tryUpdateVersionElement projectMetadata
            let bumpedProject = Bumped(projectMetadata, lang, currentVersion, nextVersion)
            return (bumpedProject, updatedProjectDocument)            
        }
    | Versioned _   -> NoCalendarVersionProject |> Error
    | Unversioned _ -> UnversionedProject       |> Error
    | Bumped _      -> AlreadyBumpedProject     |> Error
        