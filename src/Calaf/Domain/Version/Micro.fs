module internal Calaf.Domain.Micro

open Calaf.Domain.DomainTypes

[<Literal>]
let internal MicroIncrementStep = 1u
[<Literal>]
let internal MicroStartValue = 1u
    
let tryParseFromString (micro: string) : Micro option =
    match System.UInt32.TryParse(micro) with
    | true, micro when micro > 0u -> Some micro
    | _ -> None
    
let release (micro: Micro option) : Micro =    
    match micro with
    | Some m when m < Micro.MaxValue ->
        m + MicroIncrementStep
    | Some _ -> MicroStartValue
    | None   -> MicroStartValue