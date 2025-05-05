module internal Calaf.Domain.Month

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors

[<Literal>]
let lowerMonthBoundary= 1uy
[<Literal>]
let upperMonthBoundary = 12uy

let private tryParseMonth (month: System.Byte) : Result<Month, CalafError> =    
    match month with
    | month when month >= lowerMonthBoundary &&
                 month <= upperMonthBoundary -> Ok month
    | _ -> OutOfRangeMonth |> Validation |> Error

let tryParseFromInt32 (month: System.Int32) : Result<Month, CalafError> =
    try
        month
        |> System.Convert.ToByte
        |> tryParseMonth
    with _ -> WrongInt32Month |> Validation |> Error
        
let tryParseFromString (month: string) : Result<Month, CalafError> =
    match month |> System.Byte.TryParse with
    | true, month ->
        month
        |> tryParseMonth
    | _ -> WrongStringMonth |> Validation |> Error