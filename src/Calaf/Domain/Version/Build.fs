module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let private NightlyBuildType =
    "nightly"
[<Literal>]
let internal AllowedBuildRegexString =
    @"^(?i:(nightly))\.(0*(?:25[0-5]|2[0-4]\d|1\d{2}|[1-9]\d|\d))\+([A-Za-z0-9]+)$"
let private buildRegex =
    System.Text.RegularExpressions.Regex(
        AllowedBuildRegexString,
        System.Text.RegularExpressions.RegexOptions.Compiled |||
        System.Text.RegularExpressions.RegexOptions.IgnoreCase)
    
type private BuildSegments = {
    BuildType: string
    BuildNumber: string
    BuildHash: string
}

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)
    
let private tryCreateBuildSegments (buildString: string) =
    if buildString |> isEmptyString
    then
        Ok None
    else        
        let m = buildRegex.Match(buildString)
        if m.Success
        then        
            {                 
                BuildType = m.Groups[1].Value
                BuildNumber = m.Groups[2].Value
                BuildHash = m.Groups[3].Value
            } |> Some |> Ok
        else
            BuildInvalidString |> Error

let private tryParseFromBuildSegments = function
    | None -> Ok None
    | Some { BuildType = buildType; BuildNumber = number; BuildHash = hash }
        when String.Equals(buildType, NightlyBuildType, StringComparison.InvariantCultureIgnoreCase) ->
        match Byte.TryParse number with
        | true, buildNumber -> Ok (Some (Build.Nightly (buildNumber, hash)))
        | _ -> Error BuildInvalidString
    | _ -> Error BuildInvalidString
    
let tryParseFromString (build: string) =
    build |> tryCreateBuildSegments |> Result.bind tryParseFromBuildSegments 

let tryNightly (currentBuild: Build option) (newBuildMetadata: BuildNumber * BuildHash) : Result<Build, DomainError> =
    match currentBuild with
    | Some (Build.Nightly (currentBuildNumber, currentBuildHash)) ->
        let sameMetadata = newBuildMetadata = (currentBuildNumber, currentBuildHash)        
        if sameMetadata
        then BuildAlreadyCurrent |> Error
        else Build.Nightly newBuildMetadata |> Ok
    | None ->
        Ok (Build.Nightly newBuildMetadata)