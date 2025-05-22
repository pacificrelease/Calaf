module internal Calaf.Domain.Project
    
open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions
open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities

module internal Schema =    
    [<Literal>]
    let private VersionXElementName = "Version"
    [<Literal>]
    let private PropertyGroupXElementName = "PropertyGroup"    
    
    // TODO: Use ERROR instead of option?
    let tryExtractVersionElement (content: System.Xml.Linq.XElement) =
        content.Elements(PropertyGroupXElementName)
        |> Seq.collect _.Elements(VersionXElementName)
        |> Seq.tryExactlyOne
        
    let tryUpdateVersionElement (content: System.Xml.Linq.XElement) (version: string)=
        option {            
            // TODO: Maybe better to divide variables, then update and return projectDocument
            return! content.Elements(PropertyGroupXElementName)
            |> Seq.map _.Elements(VersionXElementName)
            |> Seq.tryExactlyOne
            |> Option.bind Seq.tryHead
            |> Option.map (fun versionElement -> versionElement.SetValue(version); Xml content)
        } 

let tryCapture (projectInfo: ProjectXmlFileInfo) : Project option =        
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
    |> Seq.filter (function
        | Versioned { Version = CalVer _ }  -> true
        | _ -> false)
    
let chooseXmlVCalendarVersionedProjects (projects: Project seq) =
    projects
    |> Seq.choose (function
        | Versioned { Version = CalVer _;  Content = Xml xmlContent; Metadata = m } ->
            Some {| AbsolutePath = m.AbsolutePath; Content = xmlContent |}
        | _ -> None)

let chooseCalendarVersions (projects: Project seq) : CalendarVersion seq =
    projects
    |> Seq.choose (function
        | Versioned { Version = CalVer version } -> Some version
        | _ -> None)
    
let tryBump (project: VersionedProject) (nextVersion: CalendarVersion) =    
    match project.Content with
    | Xml xmlContent ->
        Version.toString nextVersion
        |> Schema.tryUpdateVersionElement xmlContent
        |> Option.map (fun upc ->
            { project with Content = upc; Version = CalVer nextVersion })
        |> Option.toResult (NotFoundXmlVersionElement project.Metadata.Name)
    | Json _ -> Ok project