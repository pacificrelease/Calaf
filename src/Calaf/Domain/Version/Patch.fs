module internal Calaf.Domain.Patch

open Calaf.Domain.DomainTypes.Values
    
let tryParseFromString (patch: string) : Patch option =
    match System.UInt32.TryParse(patch) with
    | true, patch when patch > 0u -> Some patch
    | _ -> None
    
let bump (patch: Patch option) : Patch =
    let incrementStep = 1u
    match patch with
    | Some p when p < Patch.MaxValue -> p + incrementStep
    | Some _ -> incrementStep
    | None -> incrementStep