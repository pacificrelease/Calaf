module internal Calaf.Domain.Project
    
open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes

module XmlSchema =
    [<Literal>]
    let ProjectXElementName = "Project"
    [<Literal>]
    let VersionXElementName = "Version"
    [<Literal>]
    let PropertyGroupXElementName = "PropertyGroup"    
    
    // TODO: Use ERROR instead of option?
    let tryExtractVersionElement (content: System.Xml.Linq.XElement) =
        content.Elements(PropertyGroupXElementName)
        |> Seq.collect _.Elements(VersionXElementName)
        |> Seq.tryExactlyOne
        
    let tryUpdateVersionElement (content: System.Xml.Linq.XElement) (version: string)=
        try
            tryExtractVersionElement content
            |> Option.map (fun versionElement ->
                versionElement.SetValue(version)
                content)
            |> Option.toResult VersionElementMissing
        with
        | _ -> VersionElementUpdateFailed |> Error
        
let private isCalendarVersion (project: Project) : bool =
    match project with
    | Versioned { Version = CalVer _ } -> true
    | _ -> false
    
let private getCalendarVersion (project: Project) : CalendarVersion option =
    match project with
    | Versioned { Version = CalVer version } -> Some version
    | _ -> None

let tryCapture (projectInfo: ProjectXmlFileInfo) : Project option =        
    let tryExtractVersion (xml: System.Xml.Linq.XElement) : Version option =
        xml
        |> XmlSchema.tryExtractVersionElement
        |> Option.bind (fun x -> x.Value |> Version.tryParseFromString)        
    option {
        let metadata =
            { Name = projectInfo.Name
              Directory = projectInfo.Directory
              AbsolutePath = projectInfo.AbsolutePath
              Extension = projectInfo.Extension }        
        let! language = Language.tryParse metadata.Extension
        let version = projectInfo.Content |> tryExtractVersion        
        return
            match version with
            | Some version ->
                let content = Xml projectInfo.Content
                Versioned { Metadata = metadata; Content = content; Language = language; Version = version }
            | None -> Unversioned { Metadata = metadata; Language = language }
    }
    
let chooseCalendarVersioned (projects: Project seq) : Project seq =
    projects
    |> Seq.filter isCalendarVersion
    
let chooseCalendarVersions (projects: Project seq) : CalendarVersion seq =
    projects
    |> Seq.choose getCalendarVersion
    
let trySnapshot (project: Project) =
    match project with    
    | Versioned { Version = CalVer _;  Content = Xml xmlContent; Metadata = m } ->
        Some { AbsolutePath = m.AbsolutePath; Content = xmlContent }
    | _ -> None
    
let tryRelease (project: VersionedProject) (nextVersion: CalendarVersion) =
    match project with
    | { Version = CalVer _; Content = Xml xmlContent } ->
        Version.toString nextVersion
        |> XmlSchema.tryUpdateVersionElement xmlContent
        |> Result.map (fun xmlContent' ->
            { project with Content = Xml xmlContent'; Version = CalVer nextVersion })
    | { Version = CalVer _; Content = Json _ } -> Ok project
    | _ -> Error ProjectUnsupported