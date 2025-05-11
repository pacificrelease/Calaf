module internal Calaf.Domain.Project
    
open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes

module internal Schema =
    
    [<Literal>]
    let private VersionXElementName = "Version"
    [<Literal>]
    let private PropertyGroupXElementName = "PropertyGroup"    
    
    // TODO: Use ERROR instead of option?
    let tryExtractVersionElement (projectDocument: System.Xml.Linq.XElement) =
        projectDocument.Elements(PropertyGroupXElementName)
        |> Seq.collect _.Elements(VersionXElementName)
        |> Seq.tryExactlyOne
        
    let tryUpdateVersionElement (projectDocument: System.Xml.Linq.XElement) (version: string)=
        option {
            // TODO: Maybe better to divide variables, then update and return projectDocument
            return! projectDocument.Elements(PropertyGroupXElementName)
            |> Seq.map _.Elements(VersionXElementName)
            |> Seq.tryExactlyOne
            |> Option.bind Seq.tryHead
            |> Option.map (fun versionElement -> versionElement.SetValue(version); projectDocument)
        }
    
let choosePending (projects: (Project * System.Xml.Linq.XElement) seq) : (Project * System.Xml.Linq.XElement) seq =
    projects
    |> Seq.choose (fun (p, x) ->
        match p with
        | Versioned(_, _, CalVer _) as project -> Some (project, x)
        | _ -> None)

let chooseCalendarVersions (projects: Project seq) : CalendarVersion seq =
    projects
    |> Seq.choose (function
        | Versioned (_, _, CalVer version) -> Some version
        | _                                -> None)
    
let tryCreate (projectInfo: ProjectInfo) : Project option =        
    let tryExtractVersion (xml: System.Xml.Linq.XElement) : Version option =
        xml
        |> Schema.tryExtractVersionElement
        |> Option.bind (fun x -> x.Value |> Version.tryParseFromString)
        
    option {
        let metadata =
            { Name = projectInfo.Name
              Directory = projectInfo.Directory
              AbsolutePath = projectInfo.AbsolutePath
              Extension = projectInfo.Extension }
        let! language = Language.tryParse metadata.Extension
        let version = projectInfo.Payload |> tryExtractVersion
        return
            match version with
            | Some version -> Versioned(metadata, language, version)
            | None -> Unversioned(metadata, language)
    }
    
let tryBump (projectDocument: System.Xml.Linq.XElement) (project: Project) (nextVersion: CalendarVersion) =    
    match project with
    | Versioned(pm, lang, CalVer _) ->
        Schema.tryUpdateVersionElement projectDocument (Version.toString nextVersion)
        |> Option.map (fun updated ->
            let bumped = Versioned(pm, lang, CalVer nextVersion)
            (bumped, updated))
        |> Option.toResult (XElementUpdateFailure pm.Name)
    | Versioned   _ -> (project, projectDocument) |> Ok
    | Unversioned _ -> UnversionedProject |> Error
        