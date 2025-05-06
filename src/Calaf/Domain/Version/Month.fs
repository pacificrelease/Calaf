module internal Calaf.Domain.Month

open Calaf.Domain.DomainErrors
open Calaf.Domain.DomainTypes

[<Literal>]
let lowerMonthBoundary= 1uy
[<Literal>]
let upperMonthBoundary = 12uy

let private tryParseMonth (month: System.Byte) : Result<Month, DomainError> =    
    match month with
    | month when month >= lowerMonthBoundary &&
                 month <= upperMonthBoundary -> Ok month
    | _ -> OutOfRangeMonth |> Error

let tryParseFromInt32 (month: System.Int32) : Result<Month, DomainError> =
    try
        month
        |> System.Convert.ToByte
        |> tryParseMonth
    with _ -> WrongInt32Month |> Error
        
let tryParseFromString (month: string) : Result<Month, DomainError> =
    match month |> System.Byte.TryParse with
    | true, month ->
        month
        |> tryParseMonth
    | _ -> WrongStringMonth |> Error