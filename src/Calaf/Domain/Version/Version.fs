module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions.RegularExpressions
open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal YearMonthDivider =
    "."
[<Literal>]
let internal MonthPatchDivider =
    "."
[<Literal>]
let internal CalendarVersionBuildTypeDivider =
    "-"
let private allowedVersionRegexString =
    $@"^(\d+)\.(\d+)(?:\.(\d+))?(?:\{CalendarVersionBuildTypeDivider}(.*))?$"
[<Literal>]
let private ChoreCommitPrefix =
    "chore: "
let private matchVersionRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedVersionRegexString)
let internal versionPrefixes =
    [ "version."; "ver."; "v."
      "Version."; "Ver."; "V."
      "version";  "ver";  "v"
      "Version";  "Ver";  "V" ]
    |> List.sortByDescending String.length
let internal tagVersionPrefix =
    $"{versionPrefixes[10]}"
let internal commitVersionPrefix =    
    $"{ChoreCommitPrefix}{versionPrefixes[2]} "
    
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
       
let private shouldChange (versionUnit, calendarUnit) =
    calendarUnit <> versionUnit
       
let private tryCleanString (bareString: string) =
    let asciiWs = set [' '; '\t'; '\n'; '\r']
    if System.String.IsNullOrWhiteSpace bareString then
        None
    else
        bareString.Trim() |> String.filter (asciiWs.Contains >> not) |> Some
            
let private tryCreateVersionSegments (cleanString: CleanString) =
    let m = matchVersionRegex cleanString
    if m.Success
    then
        {
            YearOrMajor = m.Groups[1].Value
            MonthOrMinor = m.Groups[2].Value
            Patch = if m.Groups[3] |> validGroupValue then Some m.Groups[3].Value else None
            Build = if m.Groups[4] |> validGroupValue then Some m.Groups[4].Value else None
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
                let calVer = CalVer({ Year = year; Month = month; Patch = patch; Build = build })
                return Some calVer
            | _ ->
                match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    let semVer = SemVer({ Major = major; Minor = minor; Patch = patch })
                    return Some semVer
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
                let calVer = CalVer({ Year = year; Month = month; Patch = None; Build = build })
                return Some calVer
            | _ ->
                return Some Unsupported                
    }    

// TODO: Refactor to return Error instead of Option  
let private tryParse (cleanVersion: CleanString) : Version option =    
    let segments = cleanVersion |> tryCreateVersionSegments
    match segments with
    | Some segments -> segments |> tryCreateVersion |> Result.toOption |> Option.flatten
    | None -> Some Unsupported
    
let private patchRelease (currentVersion: CalendarVersion) (build: Build option) =
    match currentVersion with
    | { Build = Some (Build.Nightly _) } ->
        { Year = currentVersion.Year
          Month = currentVersion.Month
          Patch = currentVersion.Patch
          Build = build }
    | _ ->
        { Year = currentVersion.Year
          Month = currentVersion.Month
          Patch = currentVersion.Patch |> Patch.release |> Some
          Build = build }

let toString (calVer: CalendarVersion) : string =
    let calVerStr =
        match calVer.Patch with
        | Some patch ->        
            $"{calVer.Year}{YearMonthDivider}{calVer.Month}{MonthPatchDivider}{patch}"
        | None ->
            $"{calVer.Year}{YearMonthDivider}{calVer.Month}"
    let sb = System.Text.StringBuilder(calVerStr)
    match calVer.Build with
    | Some build ->
        let buildStr = Build.toString build
        let sb = sb.Append(CalendarVersionBuildTypeDivider).Append(buildStr)
        sb.ToString()
    | None ->
        sb.ToString()
    
/// <summary>
/// Converts a CalendarVersion to a Git tag name string.
/// </summary>
/// <param name="calVer">Calendar version to convert</param>
let toTagName (calVer: CalendarVersion) : string =    
    $"{tagVersionPrefix}{toString calVer}"

/// <summary>
/// Converts a CalendarVersion to a Git commit message string.
/// </summary>
/// <param name="calVer">Calendar version to convert</param>
let toCommitMessage (calVer: CalendarVersion) : string =
    $"{commitVersionPrefix}{toString calVer}"
    
let nightly (currentVersion: CalendarVersion) (dayOfMonth: DayOfMonth, monthStamp: MonthStamp) : CalendarVersion =
    let build = Build.nightly currentVersion.Build dayOfMonth |> Some
    let yearRelease = shouldChange (currentVersion.Year, monthStamp.Year)
    if yearRelease
    then
        { Year = monthStamp.Year
          Month = monthStamp.Month
          Patch = None
          Build = build }
    else
        let monthRelease = shouldChange (currentVersion.Month, monthStamp.Month)
        if monthRelease
        then
            { Year = currentVersion.Year
              Month = monthStamp.Month
              Patch = None
              Build = build }
        else
            patchRelease currentVersion build

let stable (currentVersion: CalendarVersion) (monthStamp: MonthStamp) : CalendarVersion =
    let yearRelease = shouldChange (currentVersion.Year, monthStamp.Year)
    if yearRelease
    then
        { Year = monthStamp.Year
          Month = monthStamp.Month
          Patch = None
          Build = None }
    else
        let monthRelease = shouldChange (currentVersion.Month, monthStamp.Month)
        if monthRelease
        then
            { Year = currentVersion.Year
              Month = monthStamp.Month
              Patch = None
              Build = None }
        else
            patchRelease currentVersion None

let tryMax (versions: CalendarVersion seq) : CalendarVersion option =
    match versions with
    | _ when Seq.isEmpty versions -> None
    | _ ->
        let maxVersion =
            versions
            |> Seq.maxBy (fun v ->
                match v.Build with
                | Some build ->
                    match build with
                    | Build.Nightly nightlyBuild ->
                        let priority = 1
                        (v.Year, v.Month, v.Patch, priority, nightlyBuild.Day, nightlyBuild.Number)                    
                | None ->
                    let priority = 0
                    let day = 1uy
                    let number = 0us
                    (v.Year, v.Month, v.Patch, priority, day, number))
        Some maxVersion

let tryParseFromString (bareVersion: string) : Version option =
    option {
        let! cleanString = tryCleanString bareVersion
        return! cleanString |> tryParse
    }
    
let tryParseFromTag (bareVersion: string) : Version option =
   option {
       let! cleanString = tryCleanString bareVersion
       return! cleanString |> stripVersionPrefix |> tryParse
   }