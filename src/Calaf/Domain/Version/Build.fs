module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let private NightlyBuildType =
    "nightly"
[<Literal>]
let internal BuildTypeDayDivider =
    "."
[<Literal>]
let internal DayNumberDivider =
    "."
let internal AllowedNightlyBuildRegexString =
    $@"^(?i:({NightlyBuildType}))\{BuildTypeDayDivider}(0?[1-9]|1[0-9]|2[0-9]|3[01])\{DayNumberDivider}(0*[1-9][0-9]{{0,4}})$"
let internal AllowedNightlyBuildRegexString2 =
    @"^(?i:(nightly))\.(0?[1-9]|1[0-9]|2[0-9]|3[01])\.([0-9]{1,3})$"
let private buildRegex =
    System.Text.RegularExpressions.Regex(AllowedNightlyBuildRegexString)
let private buildRegex2 =
    System.Text.RegularExpressions.Regex(AllowedNightlyBuildRegexString2)
    
type private BuildSegments = {
    BuildType:   string
    BuildDay:    string
    BuildNumber: string
}

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)
    
let private isNightlyString (buildType: string) =
    String.Equals(buildType, NightlyBuildType, StringComparison.InvariantCultureIgnoreCase)    
    
let private tryCreateBuildSegments (buildString: string) =
    result {
        if isEmptyString buildString
        then
            return None
        else
            let m = buildRegex.Match buildString
            if m.Success
            then
                let segments =
                    { BuildType   = m.Groups[1].Value
                      BuildDay    = m.Groups[2].Value
                      BuildNumber = m.Groups[3].Value }
                return Some segments                
            else        
                return! Error BuildInvalidString
    }

let private tryParseFromBuildSegments segments =
    match segments with    
    | Some { BuildType = buildType; BuildDay = day; BuildNumber = number }
        when buildType |> isNightlyString ->
        match (Byte.TryParse day, UInt16.TryParse number) with
        | (true, buildDay), (true, buildNumber) ->
            let nightly = { Day = buildDay; Number = buildNumber } |> Build.Nightly
            nightly |> Some |> Ok
        | _ -> Error BuildInvalidString
    | None -> Ok None
    | _ -> Error BuildInvalidString
    
let toString (build: Build) : string =    
    match build with
    | Build.Nightly { Day = day; Number = number } ->
        $"{NightlyBuildType}{BuildTypeDayDivider}{day:D2}{DayNumberDivider}{number:D3}"
    
let tryParseFromString (build: string) =
    build |> tryCreateBuildSegments |> Result.bind tryParseFromBuildSegments 

let tryNightly (currentBuild: Build option) (newNightly: NightlyBuild) : Result<Build, DomainError> =
    match currentBuild with
    | Some (Build.Nightly currentNightly) ->
        let same = newNightly = currentNightly
        if same
        then BuildAlreadyCurrent |> Error
        else Build.Nightly newNightly |> Ok
    | None ->
        Ok (Build.Nightly newNightly)