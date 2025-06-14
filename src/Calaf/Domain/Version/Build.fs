module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions.RegularExpressions
open Calaf.Domain.DomainTypes.Values

[<Literal>]
let private NightlyBuildType =
    "nightly"
[<Literal>]
let internal BuildTypeDayDivider = "."
[<Literal>]
let internal DayNumberDivider = "."
[<Literal>]
let internal NumberHashDivider = "+"
let internal AllowedNightlyBuildRegexString =
    $@"^(?i:({NightlyBuildType}))\{BuildTypeDayDivider}(0?[1-9]|1[0-9]|2[0-9]|3[01])\{DayNumberDivider}([0-9]{{1,3}})(?:\{NumberHashDivider}([A-Za-z0-9]{{1,512}}))?$"
let internal AllowedNightlyBuildRegexString2 =
    @"^(?i:(nightly))\.(0?[1-9]|1[0-9]|2[0-9]|3[01])\.([0-9]{1,3})(?:\+([A-Za-z0-9]{1,512}))?$"
let private buildRegex =
    System.Text.RegularExpressions.Regex(AllowedNightlyBuildRegexString)
let private buildRegex2 =
    System.Text.RegularExpressions.Regex(AllowedNightlyBuildRegexString2)
    
type private BuildSegments = {
    BuildType:   string
    BuildDay:    string
    BuildNumber: string
    BuildHash:   string option
}

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)
    
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
                      BuildNumber = m.Groups[3].Value
                      BuildHash   = if m.Groups[4] |> validGroupValue then Some m.Groups[4].Value else None }
                return Some segments                
            else        
                return! Error BuildInvalidString
    }

let private tryParseFromBuildSegments = function
    | None -> Ok None
    | Some { BuildType = buildType; BuildDay = day; BuildNumber = number; BuildHash = hash }
        when String.Equals(buildType, NightlyBuildType, StringComparison.InvariantCultureIgnoreCase) ->
        match (Byte.TryParse day, Byte.TryParse number) with
        | (true, buildDay), (true, buildNumber) ->
            Ok (Some (Build.Nightly { Day = buildDay; Number = buildNumber; Hash = hash }))
        | _ -> Error BuildInvalidString
    | _ -> Error BuildInvalidString
    
let toString (build: Build) : string =    
    match build with
    | Build.Nightly { Day = day; Number = number; Hash = Some hash } ->
        $"{NightlyBuildType}{BuildTypeDayDivider}{day:D2}{DayNumberDivider}{number:D3}{NumberHashDivider}{hash}"
    | Build.Nightly { Day = day; Number = number; Hash = None } ->
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
        
let tryMax (builds: Build seq) : Build option =
    match builds with
    | _ when Seq.isEmpty builds -> None
    | _ ->
        // sort by build type where f.e. Alpha is the first and by the greatest number then Nightly and by the greater number        
        let maxVersion =
            builds
            |> Seq.maxBy (fun b ->
                match b with
                // NOTE:: The higher is the first number the more important is the build
                // First digit (1 for nightly) defines comparison priority where higher number is better
                | Build.Nightly nightlyBuild -> (1, nightlyBuild.Day, nightlyBuild.Number))
        Some maxVersion