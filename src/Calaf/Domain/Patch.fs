module internal Calaf.Domain.Patch

open Calaf.Domain.DomainTypes

let bump (patch: Patch option) : Patch =
    let increment = 1u
    match patch with
    | Some patch -> (patch + increment)
    | None -> increment
    
let tryParseFromString (patch: string) : Patch option =
    match System.UInt32.TryParse(patch) with
    | true, patch when patch > 0u -> Some patch
    | _ -> None