module internal Calaf.Domain.Build

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal nightlyBuild = "nightly"


let private isNightlyString (build: string) =
    System.String.Equals(build, nightlyBuild, System.StringComparison.OrdinalIgnoreCase)
    
let private isEmptyString (build: string) =
    System.String.IsNullOrWhiteSpace(build)


let tryParseFromString (build: string) =
    match build with
    | b when b |> isNightlyString ->
        let number: BuildNumber = 1uy
        let hash: BuildHash     = ""
        Build.Nightly (number, hash) |> Some |> Ok
    | b when b |> isEmptyString -> None |> Ok
    | _ -> BuildInvalidString |> Error

let tryNightly (currentBuild: Build option) (newBuildMetadata: BuildNumber * BuildHash) : Result<Build, DomainError> =
    match currentBuild with
    | Some (Build.Nightly (currentBuildNumber, currentBuildHash)) ->
        let sameMetadata = newBuildMetadata = (currentBuildNumber, currentBuildHash)        
        if sameMetadata
        then BuildAlreadyCurrent |> Error
        else Build.Nightly newBuildMetadata |> Ok
    | None ->
        Ok (Build.Nightly newBuildMetadata)