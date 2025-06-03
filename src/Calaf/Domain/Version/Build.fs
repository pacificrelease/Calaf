module internal Calaf.Domain.Build

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal nightly = "nightly"

let createNightly () : Build =
    Build.Nightly

let tryParseFromString (build: string) : Build option =
    match build with
    | b when System.String.Equals(b, nightly, System.StringComparison.OrdinalIgnoreCase) ->
        Some Build.Nightly
    | _ -> None