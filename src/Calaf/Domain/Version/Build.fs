module internal Calaf.Domain.Build

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal nightly = "nightly"

let private isNightlyString (build: string) =
    System.String.Equals(build, nightly, System.StringComparison.OrdinalIgnoreCase)
    
let private isEmptyString (build: string) =
    System.String.IsNullOrWhiteSpace(build)


let tryParseFromString (build: string) =
    match build with
    | b when b |> isNightlyString ->
        Build.Nightly |> Some |> Ok
    | b when b |> isEmptyString -> None |> Ok
    | _ -> BuildInvalidString |> Error        

let bump (build: Build option) : Build =
    match build with
    | Some Build.Nightly -> Build.Nightly
    | _ -> Build.Nightly