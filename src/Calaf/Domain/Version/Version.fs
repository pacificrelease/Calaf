module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal versionDivider = '.'
[<Literal>]
let internal buildDivider = '-'
let internal versionPrefixes =
    [ "version."; "ver."; "v."
      "Version."; "Ver."; "V."
      "version";  "ver";  "v"
      "Version";  "Ver";  "V" ]
    |> List.sortByDescending String.length
let internal tagVersionPrefix = versionPrefixes[10]
let internal commitVersionPrefix =
    let chore = "chore"
    $"{chore}: {versionPrefixes[2]}"
    
type private CleanString = string

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
        
let private dedicateBuild (cleanString: CleanString) =
    cleanString.Split(buildDivider, System.StringSplitOptions.RemoveEmptyEntries)    
        
let private dedicateParts (cleanString: CleanString)=    
    let parts = cleanString.Split(versionDivider, System.StringSplitOptions.RemoveEmptyEntries)
    match parts with
    | [||] -> [||]
    | _ ->
        let head = parts |> Array.take (parts.Length - 1)
        let tail = parts |> Array.last |> dedicateBuild
        Array.concat [ head; tail ]

// TODO: Refactor to return Error instead of Option  
let private tryParse (cleanVersion: CleanString) : Version option =
    option {
        //let parts = dedicateParts cleanVersion
        let parts = cleanVersion.Split('.')
        match parts with
        | [| first; second; third |] ->
            let major = tryToUInt32 first
            let minor = tryToUInt32 second
            let patch = tryToUInt32 third
            
            let year    =
                match major with
                | Some major -> major |> int32 |> Year.tryParseFromInt32
                | _ -> Year.tryParseFromString first
            let month =
                match minor with
                | Some minor -> minor |> int32 |> Month.tryParseFromInt32
                | _ -> second |> Month.tryParseFromString 
            let patch  =
                match patch with
                | Some patch -> Some patch
                | _ -> third |> Patch.tryParseFromString 
            
            match year, month, patch with
            | Ok year, Ok month, patch ->
                let calendarVersion = { Year = year; Month = month; Patch = patch }
                return CalVer(calendarVersion)
            | _ ->
            match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    let semanticVersion = { Major = major; Minor = minor; Patch = patch }
                    return SemVer(semanticVersion)
                | _ ->
                    return Unsupported
        | [| year; month |] ->
            let year  = Year.tryParseFromString year
            let month = Month.tryParseFromString month
            match year, month with
            | Ok year, Ok month ->
                return CalVer({ Year = year; Month = month; Patch = None })
            | _ ->
                return Unsupported
        | _ ->
            return Unsupported
    }

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