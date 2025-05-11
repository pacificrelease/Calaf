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
    
let chooseCalendarVersioned (projects: Project seq) : Project seq =
    projects
    |> Seq.filter (function
        | Versioned (_, _, _, CalVer _) -> true
        | _ -> false)

let chooseCalendarVersions (projects: Project seq) : CalendarVersion seq =
    projects
    |> Seq.choose (function
        | Versioned (_, _, _, CalVer version) -> Some version
        | _                                -> None)

let tryCreate (projectInfo: ProjectXmlFileInfo) : Project option =        
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
                Versioned(metadata, content, language, version)
            | None -> Unversioned(metadata, language)
    }
    
let tryBump (project: Project) (nextVersion: CalendarVersion) =    
    match project with
    | Versioned (pm, Xml pc, lang, CalVer _) ->
        Version.toString nextVersion
        |> Schema.tryUpdateVersionElement pc
        |> Option.map (fun upc ->
            Versioned(pm, upc, lang, CalVer nextVersion))
        |> Option.toResult (XElementUpdateFailure pm.Name)
    | Versioned   _ -> Ok project
    | Unversioned _ -> UnversionedProject |> Error
        