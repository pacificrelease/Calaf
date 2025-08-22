module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions.RegularExpressions
open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal YearMonthDivider =
    "."
[<Literal>]
let internal MonthMicroDivider =
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
    MicroOrPatch: string option
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
            YearOrMajor =
                m.Groups[1].Value
            MonthOrMinor =
                m.Groups[2].Value
            MicroOrPatch =
                if m.Groups[3] |> validGroupValue
                then Some m.Groups[3].Value
                else None
            Build =
                if m.Groups[4] |> validGroupValue
                then Some m.Groups[4].Value
                else None
        } |> Some
    else None    
    
let private tryCreateVersion (segments: VersionSegments) =
    result {
        match segments with
        | { YearOrMajor = yearOrMajorSegment
            MonthOrMinor = monthOrMinorSegment
            MicroOrPatch = Some microOrPatchSegment
            Build = buildSegment } ->
            let! build =
                buildSegment
                |> Option.map Build.tryParseFromString
                |> Option.defaultValue (Ok None)
           
            let major = tryToUInt32 yearOrMajorSegment
            let minor = tryToUInt32 monthOrMinorSegment
            let patch = tryToUInt32 microOrPatchSegment                
            let year =
                match major with
                | Some major -> major |> int32 |> Year.tryParseFromInt32
                | _ -> Year.tryParseFromString yearOrMajorSegment
            let month =
                match minor with
                | Some minor -> minor |> int32 |> Month.tryParseFromInt32
                | _ -> monthOrMinorSegment |> Month.tryParseFromString 
            let micro =
                match patch with
                | Some patch -> Some patch
                | _ -> microOrPatchSegment |> Micro.tryParseFromString 
            
            match year, month, micro with
            | Ok year, Ok month, micro ->
                let calVer = CalVer({ Year = year; Month = month; Micro = micro; Build = build })
                return Some calVer
            | _ ->
                match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    let semVer = SemVer({ Major = major; Minor = minor; Patch = patch })
                    return Some semVer
                | _ ->
                    return Unsupported |> Some
        | { YearOrMajor = yearOrMajorSegment
            MonthOrMinor = monthOrMinorSegment
            MicroOrPatch = None
            Build = buildSegment } ->
            let! build =
                buildSegment
                |> Option.map Build.tryParseFromString
                |> Option.defaultValue (Ok None)
                
            let year = Year.tryParseFromString yearOrMajorSegment
            let month = Month.tryParseFromString monthOrMinorSegment        
            match year, month with
            | Ok year, Ok month ->
                let calVer = CalVer({ Year = year; Month = month; Micro = None; Build = build })
                return Some calVer
            | _ ->
                return Some Unsupported                
    }    

// TODO: Refactor to return Error instead of Option  
let private tryParse (cleanVersion: CleanString) : Version option =    
    let segments = cleanVersion |> tryCreateVersionSegments
    match segments with
    | Some segments ->
        segments |> tryCreateVersion |> Result.toOption |> Option.flatten
    | None -> Some Unsupported
    
let private microRelease (currentVersion: CalendarVersion, build: Build option) =
    match currentVersion with
    | { Build = Some _ } ->
        { Year  = currentVersion.Year
          Month = currentVersion.Month
          Micro = currentVersion.Micro
          Build = build }
        
    | { Build = None } ->
        let newMicro =
            currentVersion.Micro
            |> Micro.release
            |> Some
        { Year  = currentVersion.Year
          Month = currentVersion.Month
          Micro = newMicro
          Build = build }

let toString (calVer: CalendarVersion) : string =
    let calVerStr =
        match calVer.Micro with
        | Some micro ->        
            $"{calVer.Year}{YearMonthDivider}{calVer.Month}{MonthMicroDivider}{micro}"
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
/// Converts a CalendarVersion to a Git commit text.
/// </summary>
/// <param name="calVer">Calendar version to convert</param>
let toCommitText (calVer: CalendarVersion) : string =
    $"{commitVersionPrefix}{toString calVer}"
    
let tryReleaseCandidate (currentVersion: CalendarVersion) (dateTimeOffsetStamp: System.DateTimeOffset) : Result<CalendarVersion, DomainError> =
    result {
        let! build = Build.tryReleaseCandidate currentVersion.Build
        let! stampYear = Year.tryParseFromInt32 dateTimeOffsetStamp.Year
        let! stampMonth = Month.tryParseFromInt32 dateTimeOffsetStamp.Month
        
        let shouldReleaseYear = shouldChange (currentVersion.Year, stampYear)
        let shouldReleaseMonth = shouldChange (currentVersion.Month, stampMonth)
        
        return
            match shouldReleaseYear, shouldReleaseMonth with
            | true, _ ->
                { Year = stampYear
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, true ->
                { Year = currentVersion.Year
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, false ->
                microRelease (currentVersion, Some build)
    }
    
let tryBeta (currentVersion: CalendarVersion) (dateTimeOffsetStamp: System.DateTimeOffset) : Result<CalendarVersion, DomainError> =
    result {
        let! build = Build.tryBeta currentVersion.Build
        let! stampYear = Year.tryParseFromInt32 dateTimeOffsetStamp.Year
        let! stampMonth = Month.tryParseFromInt32 dateTimeOffsetStamp.Month
        
        let shouldReleaseYear = shouldChange (currentVersion.Year, stampYear)
        let shouldReleaseMonth = shouldChange (currentVersion.Month, stampMonth)
        
        return
            match shouldReleaseYear, shouldReleaseMonth with
            | true, _ ->
                { Year  = stampYear
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, true ->
                { Year  = currentVersion.Year
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, false ->
                microRelease (currentVersion, Some build)
    }

let tryAlpha (currentVersion: CalendarVersion) (dateTimeOffsetStamp: System.DateTimeOffset) : Result<CalendarVersion, DomainError> =
    result {
        let! build = Build.tryAlpha currentVersion.Build
        let! stampYear = Year.tryParseFromInt32 dateTimeOffsetStamp.Year
        let! stampMonth = Month.tryParseFromInt32 dateTimeOffsetStamp.Month
        
        let shouldReleaseYear = shouldChange (currentVersion.Year, stampYear)
        let shouldReleaseMonth = shouldChange (currentVersion.Month, stampMonth)
        
        return
            match shouldReleaseYear, shouldReleaseMonth with
            | true, _ ->
                { Year = stampYear
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, true ->
                { Year = currentVersion.Year
                  Month = stampMonth
                  Micro = None
                  Build = Some build }
            | false, false ->
                microRelease (currentVersion, Some build)
    }
     
let tryNightly (currentVersion: CalendarVersion) (dateTimeOffsetStamp: System.DateTimeOffset) : Result<CalendarVersion, DomainError> =
    result {
        let! day = Day.tryParseFromInt32 dateTimeOffsetStamp.Day
        let! build = Build.tryNightly currentVersion.Build day
        let! stampYear = Year.tryParseFromInt32 dateTimeOffsetStamp.Year
        let! stampMonth = Month.tryParseFromInt32 dateTimeOffsetStamp.Month
        
        let shouldReleaseYear = shouldChange (currentVersion.Year, stampYear)
        if shouldReleaseYear
        then
            return { Year  = stampYear
                     Month = stampMonth
                     Micro = None
                     Build = Some build }
        else
            let shouldReleaseMonth = shouldChange (currentVersion.Month,stampMonth)
            if shouldReleaseMonth
            then
                return { Year = currentVersion.Year
                         Month = stampMonth
                         Micro = None
                         Build = Some build }
            else
                return microRelease (currentVersion, Some build)        
    }

let tryStable (currentVersion: CalendarVersion) (dateTimeOffsetStamp: System.DateTimeOffset) : Result<CalendarVersion, DomainError> =
    result {
        let noBuild = None
        let! stampYear = Year.tryParseFromInt32 dateTimeOffsetStamp.Year
        let! stampMonth = Month.tryParseFromInt32 dateTimeOffsetStamp.Month
        
        let yearRelease = shouldChange (currentVersion.Year, stampYear)
        if yearRelease
        then
            return { Year = stampYear
                     Month = stampMonth
                     Micro = None
                     Build = noBuild }
        else
            let monthRelease = shouldChange (currentVersion.Month, stampMonth)
            if monthRelease
            then
                return { Year = currentVersion.Year
                         Month = stampMonth
                         Micro = None
                         Build = noBuild }
            else            
                return microRelease (currentVersion, noBuild)
    }

let tryMax (versions: CalendarVersion seq) : CalendarVersion option =
    let noPreReleaseNumber = 0us
    let noNightlyDay       = 0uy
    let noNightlyNumber    = 0us    
    match versions with
    | _ when Seq.isEmpty versions -> None
    | _ ->
        let maxVersion =
            versions
            |> Seq.maxBy (fun v ->
                match v.Build with
                | Some build ->
                    match build with
                    | Build.ReleaseCandidateNightly (rc, nightly) ->
                        let priority = 4
                        (v.Year, v.Month, v.Micro, priority, rc.Number, nightly.Day, nightly.Number)
                    | Build.ReleaseCandidate rcBuild ->
                        let priority = 4
                        (v.Year, v.Month, v.Micro, priority, rcBuild.Number, noNightlyDay, noNightlyNumber)                    
                    | Build.BetaNightly (beta, nightly) ->
                        let priority = 3
                        (v.Year, v.Month, v.Micro, priority, beta.Number, nightly.Day, nightly.Number)
                    | Build.Beta betaBuild ->
                        let priority = 3                        
                        (v.Year, v.Month, v.Micro, priority, betaBuild.Number, noNightlyDay, noNightlyNumber)
                    | Build.AlphaNightly (alpha, nightly) ->
                        let priority = 2
                        (v.Year, v.Month, v.Micro, priority, alpha.Number, nightly.Day, nightly.Number)
                    | Build.Alpha alphaBuild ->
                        let priority = 2
                        (v.Year, v.Month, v.Micro, priority, alphaBuild.Number, noNightlyDay, noNightlyNumber)
                    | Build.Nightly nightlyBuild ->
                        let priority = 1
                        (v.Year, v.Month, v.Micro, priority, noPreReleaseNumber, nightlyBuild.Day, nightlyBuild.Number)
                | None ->
                    let priority = 5
                    (v.Year, v.Month, v.Micro, priority, noPreReleaseNumber, noNightlyDay, noNightlyNumber))
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