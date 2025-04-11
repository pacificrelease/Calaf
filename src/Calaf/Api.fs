// Api. Composition Root
module rec Calaf.Api

open System.IO
open FsToolkit.ErrorHandling

[<Literal>]
let searchFilesPattern = "*.?sproj"
    
let CreateWorkspace (workingDir: string) =
    let tryParseProject (metadata: ProjectMetadata) : Project option =
        option {
            let! xml = Xml.tryLoadXml(metadata.AbsolutePath)
            return! Project.tryCreate(metadata, xml)
        }        
    let loadProjects (workingDir : DirectoryInfo) =
        FileSystem.readFilesMatching searchFilesPattern workingDir
        |> Seq.map ProjectMetadata.create
        |> Seq.choose tryParseProject
        |> Seq.toArray
    
    let directory = WorkingDir.create workingDir
    let projects = directory |> loadProjects
    let version = WorkspaceVersion.create projects
    {
      Name = directory.Name
      Directory = directory.FullName
      Projects = projects
      Version = version
    }

let GetNextVersion (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
    option {
        let! propertyGroup = workspace.Version.PropertyGroup
        let! nextPropertyGroupVersion = Version.tryGetNext propertyGroup timeStamp
        let nextVersion = { PropertyGroup = Some nextPropertyGroupVersion }
        return nextVersion
    }

// Pure
module Year =
    let private tryGetYearString (year: string) : SuitableVersionPart option =
        match year with
        | year when year.Length = 4 &&
                    year |> Seq.forall System.Char.IsDigit
            -> Some year
        | _ -> None
        
    let private tryCreateYear (suitableYearString: SuitableVersionPart) : Year option =
        match System.UInt16.TryParse(suitableYearString) with
        | true, year -> Some year
        | _ -> None

    // TODO: Use ERROR instead of option
    let tryCreateFromInt32 (year: System.Int32) : Year option =
        try
            let year = System.Convert.ToUInt16(year)
            Some year
        with _ ->
            None        

    let tryCreateFromString (year: string) : Year option =
        option {
            let! suitableYearString = tryGetYearString(year)                
            return! tryCreateYear suitableYearString
        }

module Month =
    let private tryGetMonthString (month: string) : SuitableVersionPart option =
        match month with
        | month when (month.Length = 1 || month.Length = 2) &&
                      month |> Seq.forall System.Char.IsDigit
            -> Some month
        | _ -> None
        
    let private tryCreateMonth (suitableMonthString: SuitableVersionPart) : Month option =
        match System.Byte.TryParse(suitableMonthString) with
        | true, month -> Some month
        | _ -> None
        
    let tryCreateFromInt32 (month: System.Int32) : Month option =
        try
            let month = System.Convert.ToByte(month)
            Some month
        with _ ->
            None
        
    let tryCreate (month: string) : Month option =
        option {
            let! suitableMonthString = tryGetMonthString(month)                
            return! tryCreateMonth suitableMonthString
        }

module Patch =
    let bump (patch: Patch option) : Patch =
        let increment = 1u
        match patch with
        | Some patch -> (patch + increment)
        | None -> increment
        
    let tryCreate (patch: string) : Patch option =
        match System.UInt32.TryParse(patch) with
        | true, patch -> Some patch
        | _ -> None

module Version =
    let private trySeqVersionElements(xElement: System.Xml.Linq.XElement) =
        option {
            let versionElements = xElement.Elements("PropertyGroup") |> Seq.map _.Elements("Version")
            let version = versionElements |> Seq.tryHead
            return! version
        }              
        
    let private tryGetVersionString (versionElements: System.Xml.Linq.XElement seq) : string option =
        option {
            return! match versionElements with
                    | seq when Seq.isEmpty seq -> None
                    | seq when Seq.length seq > 1 -> None
                    | seq -> Some (seq |> Seq.head).Value
        }       
        
    let private tryCreateVersion (suitableVersionString: string)=
        option {
            let parts = suitableVersionString.Split('.')
            match parts with
            | [| year; month; patch |] ->
                let year = Year.tryCreateFromString year
                let month = Month.tryCreate month
                let patch = Patch.tryCreate patch
                match year, month, patch with
                | Some year, Some month, patch ->
                    return CalVer({ Year = year; Month = month; Patch = patch })
                | _ ->
                    return LooksLikeSemVer(suitableVersionString)
            | [| year; month |] ->
                let year = Year.tryCreateFromString year
                let month = Month.tryCreate month
                match year, month with
                | Some year, Some month ->
                    return CalVer({ Year = year; Month = month; Patch = None })
                | _ ->
                    return Unsupported
            | _ ->
                return Unsupported
        }
        
    let tryMax (versions: CalendarVersion[]) : CalendarVersion option =
        match versions with
        | [||] -> None
        | _ ->
            let maxVersion = versions |> Array.maxBy (fun v -> v.Year, v.Month, v.Patch)
            Some maxVersion
     
     // TODO: Use ERROR instead of option
    let tryGetNext (currentVersion: CalendarVersion) (timeStamp: System.DateTime) : CalendarVersion option =
        option {
            let! tsYear = Year.tryCreateFromInt32 timeStamp.Year
            let! tsMonth = Month.tryCreateFromInt32 timeStamp.Month
            
            let bumpYear = tsYear > currentVersion.Year            
            if bumpYear then
                return { Year = tsYear; Month = tsMonth; Patch = None }
            else
                let bumpMonth = tsMonth > currentVersion.Month
                if bumpMonth then
                    return { Year = currentVersion.Year; Month = tsMonth; Patch = None }
                else
                    let patch = currentVersion.Patch |> Patch.bump |> Some
                    return { Year = currentVersion.Year; Month = currentVersion.Month; Patch = patch }
        }
        
    let tryCreate (xml: System.Xml.Linq.XElement) : Version option =
        option {
            let! seqVersionElements = trySeqVersionElements xml
            let! versionString = tryGetVersionString seqVersionElements
            return! tryCreateVersion versionString
        }
        
module WorkspaceVersion =
    let private tryCreatePropertyGroupVersion (projects: Project[]) : CalendarVersion option =
        Project.chooseCalendarVersions projects
        |> Version.tryMax
        
    let create (projects: Project[]) : WorkspaceVersion =
        let propertyGroup = tryCreatePropertyGroupVersion projects
        {
          PropertyGroup = propertyGroup
        }

module Language =
    let tryCreate ext =
        match ext with
        | ".fsproj" -> Some(Language.FSharp)
        | ".csproj" -> Some(Language.CSharp)
        | _         -> None

module ProjectMetadata =
    let create (fileInfo: FileInfo) : ProjectMetadata =
        {
          Name = fileInfo.Name            
          Directory = fileInfo.DirectoryName
          AbsolutePath = fileInfo.FullName
          Extension = fileInfo.Extension
        }

module Project =
    let choosePending (projects: Project[]) : Project[] =
        projects
        |> Array.choose (function
            | Versioned (_, _, CalVer _) as project -> Some project
            //| Bumped _ as project -> Some project
            | _ -> None)

    let chooseCalendarVersions (projects: Project[]) : CalendarVersion[] =
        projects
        |> Array.choose (function
            | Versioned (_, _, CalVer version) -> Some version
            | Bumped (_, _, _, version) -> Some version
            | _ -> None)        
        
    let tryCreate (metadata: ProjectMetadata, xml: System.Xml.Linq.XElement) : Project option =
        option {            
            let! language = Language.tryCreate metadata.Extension
            let version = Version.tryCreate xml
            return
                match version with
                | Some version -> Versioned(metadata, language, version)
                | None   -> Eligible(metadata, language)
        }

module WorkingDir =
    let private workingDirOrDefault workingDir =
        defaultArg workingDir "."

    let private workingDirOrDefault2 workingDir =        
        if System.String.IsNullOrWhiteSpace workingDir then "." else workingDir

    let create(workingDir: string) =
        workingDir
        |> workingDirOrDefault2
        |> DirectoryInfo            

// Impure
module FileSystem =    
    let readFilesMatching (pattern: string) (workingDir: DirectoryInfo) : FileInfo[] =
        workingDir.GetFiles(pattern, SearchOption.AllDirectories)
module Xml =        
    let tryLoadXml (absolutePath: string) : System.Xml.Linq.XElement option =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)
            Some xml
        with _ ->
            None