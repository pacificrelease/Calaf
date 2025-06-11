module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let private AllowedVersionRegexString =
    @"^(\d+)\.(\d+)(?:\.(\d+))?(?:-(.*))?$"
[<Literal>]
let private ChoreCommitPrefix =
    "chore: "
let private versionRegex =
    System.Text.RegularExpressions.Regex(
        AllowedVersionRegexString,
        System.Text.RegularExpressions.RegexOptions.Compiled |||
        System.Text.RegularExpressions.RegexOptions.IgnoreCase)    
let internal versionPrefixes =
    [ "version."; "ver."; "v."
      "Version."; "Ver."; "V."
      "version";  "ver";  "v"
      "Version";  "Ver";  "V" ]
    |> List.sortByDescending String.length
let internal tagVersionPrefix = versionPrefixes[10]
let internal commitVersionPrefix =    
    $"{ChoreCommitPrefix}: {versionPrefixes[2]}"
    
type private CleanString = string
type private VersionSegments = {
    YearOrMajor: string
    MonthOrMinor: string
    Patch: string option
    Build: string option
}

let private tryToUInt32 (versionPart: string) : uint32 option =
    match System.UInt32.TryParse versionPart with
    | true, versionPart -> Some versionPart
    | _ -> None
    
let private stripVersionPrefix (tagString: CleanString) =
    versionPrefixes
    |> List.tryFind (fun s -> tagString.StartsWith(s, System.StringComparison.InvariantCultureIgnoreCase))
    |> function
       | Some p -> tagString.Substring(p.Length)
       | None   -> tagString
       
let private tryCleanString (bareString: string) =
    let asciiWs = set [' '; '\t'; '\n'; '\r']
    if System.String.IsNullOrWhiteSpace bareString then
        None
    else
        bareString.Trim() |> String.filter (asciiWs.Contains >> not) |> Some
    
let private isValidGroupValue (group: System.Text.RegularExpressions.Group) =
    group.Success && not (System.String.IsNullOrWhiteSpace group.Value)
            
let private tryCreateVersionSegments (cleanString: CleanString) =
    let m = versionRegex.Match(cleanString)
    if m.Success
    then
        {
            YearOrMajor = m.Groups[1].Value
            MonthOrMinor = m.Groups[2].Value
            Patch = if isValidGroupValue m.Groups[3] then Some m.Groups[3].Value else None
            Build = if isValidGroupValue m.Groups[4] then Some m.Groups[4].Value else None
        } |> Some
    else None    
    
let private tryCreateVersion (segments: VersionSegments) =
    result {
        match segments with
        | { YearOrMajor = yearOrMajorSegment; MonthOrMinor = monthOrMinorSegment; Patch = Some patchSegment; Build = buildSegment } ->
            let! build =
                buildSegment
                |> Option.map Build.tryParseFromString
                |> Option.defaultValue (Ok None)
           
            let major = tryToUInt32 yearOrMajorSegment
            let minor = tryToUInt32 monthOrMinorSegment
            let patch = tryToUInt32 patchSegment                
            let year =
                match major with
                | Some major -> major |> int32 |> Year.tryParseFromInt32
                | _ -> Year.tryParseFromString yearOrMajorSegment
            let month =
                match minor with
                | Some minor -> minor |> int32 |> Month.tryParseFromInt32
                | _ -> monthOrMinorSegment |> Month.tryParseFromString 
            let patch =
                match patch with
                | Some patch -> Some patch
                | _ -> patchSegment |> Patch.tryParseFromString 
            
            match year, month, patch with
            | Ok year, Ok month, patch ->
                return CalVer({ Year = year; Month = month; Patch = patch }) |> Some
            | _ ->
                match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    return SemVer({ Major = major; Minor = minor; Patch = patch }) |> Some
                | _ ->
                    return Unsupported |> Some
        | { YearOrMajor = yearOrMajorSegment; MonthOrMinor = monthOrMinorSegment; Patch = None; Build = buildSegment } ->
            let! build =
                buildSegment
                |> Option.map Build.tryParseFromString
                |> Option.defaultValue (Ok None)
                
            let year = Year.tryParseFromString yearOrMajorSegment
            let month = Month.tryParseFromString monthOrMinorSegment        
            match year, month with
            | Ok year, Ok month ->
                return CalVer({ Year = year; Month = month; Patch = None }) |> Some
            | _ ->
                return Some Unsupported                
    }    

// TODO: Refactor to return Error instead of Option  
let private tryParse (cleanVersion: CleanString) : Version option =    
    let segments = cleanVersion |> tryCreateVersionSegments
    match segments with
    | Some segments -> segments |> tryCreateVersion |> Result.toOption |> Option.flatten
    | None -> Some Unsupported

let toString (calVer: CalendarVersion) : string =
    match calVer.Patch with
    | Some patch -> $"{calVer.Year}.{calVer.Month}.{patch}"
    | None -> $"{calVer.Year}.{calVer.Month}"
    
/// <summary>
/// Converts a CalendarVersion to a Git tag name string.
/// </summary>
/// <param name="calVer">Calendar version to convert</param>
let toTagName (calVer: CalendarVersion) : string =    
    tagVersionPrefix + toString calVer

/// <summary>
/// Converts a CalendarVersion to a Git commit message string.
/// </summary>
/// <param name="calVer">Calendar version to convert</param>
let toCommitMessage (calVer: CalendarVersion) : string =
    $"{commitVersionPrefix} {calVer |> toString}" 

let release (currentVersion: CalendarVersion) (monthStamp: MonthStamp) : CalendarVersion =    
    let shouldBumpYear = monthStamp.Year > currentVersion.Year            
    if shouldBumpYear then
        { Year = monthStamp.Year
          Month = monthStamp.Month
          Patch = None }
    else
        let shouldBumpMonth = monthStamp.Month > currentVersion.Month
        if shouldBumpMonth then
            { Year = currentVersion.Year
              Month = monthStamp.Month
              Patch = None }
        else
            let patch = currentVersion.Patch |> Patch.release |> Some
            { Year = currentVersion.Year
              Month = currentVersion.Month
              Patch = patch }           
    
let tryMax (versions: CalendarVersion seq) : CalendarVersion option =
    match versions with
    | _ when Seq.isEmpty versions -> None
    | _ ->
        let maxVersion = versions |> Seq.maxBy (fun v -> v.Year, v.Month, v.Patch)
        Some maxVersion

let tryParseFromString (bareVersion: string) : Version option =
    option {
        let! cleanVersion = tryCleanString bareVersion 
        return! cleanVersion |> tryParse
    }
    
let tryParseFromTag (bareVersion: string) : Version option =
   option {
       let! cleanString = tryCleanString bareVersion 
       return! cleanString
        |> stripVersionPrefix
        |> tryParse
   }