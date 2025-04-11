// Pure
module rec Calaf.Functions

open System.IO
open FsToolkit.ErrorHandling

module Year =
    let private tryParseYearString (year: string) : SuitableVersionPart option =
        match year with
        | year when year.Length = 4 &&
                    year |> Seq.forall System.Char.IsDigit
            -> Some year
        | _ -> None
        
    let private tryParseYear (suitableYearString: SuitableVersionPart) : Year option =
        match System.UInt16.TryParse(suitableYearString) with
        | true, year -> Some year
        | _ -> None

    // TODO: Use ERROR instead of option
    let tryParseFromInt32 (year: System.Int32) : Year option =
        try
            let year = System.Convert.ToUInt16(year)
            Some year
        with _ ->
            None        

    let tryParseFromString (year: string) : Year option =
        option {
            let! suitableYearString = tryParseYearString(year)                
            return! tryParseYear suitableYearString
        }

module Month =
    let private tryParseMonthString (month: string) : SuitableVersionPart option =
        match month with
        | month when (month.Length = 1 || month.Length = 2) &&
                      month |> Seq.forall System.Char.IsDigit
            -> Some month
        | _ -> None
        
    let private tryParseMonth (suitableMonthString: SuitableVersionPart) : Month option =
        match System.Byte.TryParse(suitableMonthString) with
        | true, month -> Some month
        | _ -> None
        
    let tryParseFromInt32 (month: System.Int32) : Month option =
        try
            let month = System.Convert.ToByte(month)
            Some month
        with _ ->
            None
            
    let tryParseFromString (month: string) : Month option =
        option {            
            let! suitableMonthString = tryParseMonthString(month)                
            return! tryParseMonth suitableMonthString
        }

module Patch =
    let bump (patch: Patch option) : Patch =
        let increment = 1u
        match patch with
        | Some patch -> (patch + increment)
        | None -> increment
        
    let tryParseFromString (patch: string) : Patch option =
        match System.UInt32.TryParse(patch) with
        | true, patch -> Some patch
        | _ -> None

module Version =
    let private tryExtractVersionElements(xElement: System.Xml.Linq.XElement) =
        option {
            let versionElements = xElement.Elements("PropertyGroup") |> Seq.map _.Elements("Version")
            let version = versionElements |> Seq.tryHead
            return! version
        }              
        
    let private tryExtractVersionString (versionElements: System.Xml.Linq.XElement seq) : string option =
        option {
            return! match versionElements with
                    | seq when Seq.isEmpty seq -> None
                    | seq when Seq.length seq > 1 -> None
                    | seq -> Some (seq |> Seq.head).Value
        }       
        
    let private tryParseVersion (suitableVersionString: string)=
        option {
            let parts = suitableVersionString.Split('.')
            match parts with
            | [| year; month; patch |] ->
                let year = Year.tryParseFromString year
                let month = Month.tryParseFromString month
                let patch = Patch.tryParseFromString patch
                match year, month, patch with
                | Some year, Some month, patch ->
                    return CalVer({ Year = year; Month = month; Patch = patch })
                | _ ->
                    return LooksLikeSemVer(suitableVersionString)
            | [| year; month |] ->
                let year = Year.tryParseFromString year
                let month = Month.tryParseFromString month
                match year, month with
                | Some year, Some month ->
                    return CalVer({ Year = year; Month = month; Patch = None })
                | _ ->
                    return Unsupported
            | _ ->
                return Unsupported
        }
    
    // TODO: Use ERROR instead of option    
    let private tryBumpCurrent (currentVersion: CalendarVersion) (timeStamp: System.DateTime) : CalendarVersion option =
        option {
            let! year = Year.tryParseFromInt32 timeStamp.Year
            let! month = Month.tryParseFromInt32 timeStamp.Month            
            let bumpYear = year > currentVersion.Year            
            if bumpYear then
                return { Year = year
                         Month = month
                         Patch = None }
            else
                let bumpMonth = month > currentVersion.Month
                if bumpMonth then
                    return { Year = currentVersion.Year
                             Month = month
                             Patch = None }
                else
                    let patch = currentVersion.Patch |> Patch.bump |> Some
                    return { Year = currentVersion.Year
                             Month = currentVersion.Month
                             Patch = patch }           
        }   
        
    let tryMax (versions: CalendarVersion[]) : CalendarVersion option =
        match versions with
        | [||] -> None
        | _ ->
            let maxVersion = versions |> Array.maxBy (fun v -> v.Year, v.Month, v.Patch)
            Some maxVersion
            
    let tryCreateFromProjects (projects: Project[]) : CalendarVersion option =
        Project.chooseCalendarVersions projects
        |> tryMax
     
     // TODO: Use ERROR instead of option
    let tryGetNext (currentVersion: CalendarVersion) (timeStamp: System.DateTime) : CalendarVersion option =
        option {
            return! tryBumpCurrent currentVersion timeStamp
        }
        
    let tryParse (xml: System.Xml.Linq.XElement) : Version option =
        option {
            let! seqVersionElements = tryExtractVersionElements xml
            let! versionString = tryExtractVersionString seqVersionElements
            return! tryParseVersion versionString
        }
        
module WorkspaceVersion =
    let create (projects: Project[]) : WorkspaceVersion =
        let propertyGroup = Version.tryCreateFromProjects projects
        { PropertyGroup = propertyGroup }

module Language =
    let tryParse ext =
        match ext with
        | ".fsproj" -> Some(Language.FSharp)
        | ".csproj" -> Some(Language.CSharp)
        | _         -> None

module ProjectMetadata =
    let create (fileInfo: FileInfo) : ProjectMetadata =
        { Name = fileInfo.Name            
          Directory = fileInfo.DirectoryName
          AbsolutePath = fileInfo.FullName
          Extension = fileInfo.Extension }

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
            let! language = Language.tryParse metadata.Extension
            let version = Version.tryParse xml
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

    let create (workingDir: string) =
        workingDir
        |> workingDirOrDefault2
        |> DirectoryInfo