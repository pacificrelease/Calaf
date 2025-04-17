module internal Calaf.Domain.Month

open Calaf.Domain.DomainTypes

let tryParseFromInt32 (month: System.Int32) : Month option =
    try
        let month = System.Convert.ToByte(month)
        Some month
    with _ ->
        None
        
let tryParseFromString (month: string) : Month option =
    match System.Byte.TryParse(month) with
    | true, month when month >= byte 1 && month <= byte 12 ->
        Some month
    | _ ->
        None