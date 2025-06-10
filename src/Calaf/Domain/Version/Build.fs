module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let private NightlyBuildType =
    "nightly"
[<Literal>]
let internal AllowedBuildRegexString =
    @"^(?i:(nightly))\.([0-9]{1,3})\+([A-Za-z0-9]{1,512})$"
let private buildRegex =
    System.Text.RegularExpressions.Regex(AllowedBuildRegexString)
    
type private BuildSegments = {
    BuildType: string
    BuildNumber: string
    BuildHash: string
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
                      BuildNumber = m.Groups[2].Value
                      BuildHash   = m.Groups[3].Value }
                return Some segments                
            else        
                return! Error BuildInvalidString
    }

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