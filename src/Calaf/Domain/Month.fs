module internal Calaf.Domain.Month

open Calaf.Domain.DomainTypes

let private tryParseMonth (month: System.Byte) : Month option =
    match month with
    | month when month >= byte 1 &&
                 month <= byte 12 -> Some month
    | _ -> None

let tryParseFromInt32 (month: System.Int32) : Month option =
    try
        month
        |> System.Convert.ToByte
        |> tryParseMonth
    with _ -> None
        
let tryParseFromString (month: string) : Month option =
    match month |> System.Byte.TryParse with
    | true, month ->
        month
        |> tryParseMonth
    | _ -> None