module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes

type private CleanString = string

let private tryToUInt32 (versionPart: string) : uint32 option =
    match System.UInt32.TryParse versionPart with
    | true, versionPart -> Some versionPart
    | _ -> None
    
let private versionPrefixes =
    [ "version."; "ver."; "v."
      "Version."; "Ver."; "V."
      "version";  "ver";  "v"
      "Version";  "Ver";  "V" ]
    |> List.sortByDescending String.length
    
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
        //bareString.Trim() |> String.filter (System.Char.IsWhiteSpace >> not) |> Some
        bareString.Trim() |> String.filter (asciiWs.Contains >> not) |> Some
    
let private tryParse (cleanVersion: CleanString) : Version option =
    option {        
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
            | Some year, Some month, patch ->
                return CalVer({ Year = year; Month = month; Patch = patch })            
            | _ ->
            match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    return SemVer({ Major = major; Minor = minor; Patch = patch })
                | _ ->
                    return Unsupported
        | [| year; month |] ->
            let year    = Year.tryParseFromString year
            let month = Month.tryParseFromString month
            match year, month with
            | Some year, Some month ->
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

// TODO: Use ERROR instead of option    
let tryBump (currentVersion: CalendarVersion) (timeStamp: System.DateTime) : CalendarVersion option =
    option {
        let! year    = Year.tryParseFromInt32 timeStamp.Year
        let! month = Month.tryParseFromInt32 timeStamp.Month            
        let shouldBumpYear = year > currentVersion.Year            
        if shouldBumpYear then
            return { Year = year
                     Month = month
                     Patch = None }
        else
            let shouldBumpMonth = month > currentVersion.Month
            if shouldBumpMonth then
                return { Year = currentVersion.Year
                         Month = month
                         Patch = None }
            else
                let patch = currentVersion.Patch |> Patch.bump |> Some
                return { Year = currentVersion.Year
                         Month = currentVersion.Month
                         Patch = patch }           
    }   
    
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