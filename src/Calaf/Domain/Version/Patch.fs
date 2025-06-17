module internal Calaf.Domain.Patch

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal PatchIncrementStep = 1u
[<Literal>]
let internal PatchStartValue = 1u
    
let tryParseFromString (patch: string) : Patch option =
    match System.UInt32.TryParse(patch) with
    | true, patch when patch > 0u -> Some patch
    | _ -> None
    
let release (patch: Patch option) : Patch =    
    match patch with
    | Some p when p < Patch.MaxValue -> p + PatchIncrementStep
    | Some _ -> PatchStartValue
    | None -> PatchStartValue