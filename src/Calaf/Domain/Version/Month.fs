module internal Calaf.Domain.Month

open Calaf.Domain.DomainTypes

[<Literal>]
let internal LowerMonthBoundary= 1uy
[<Literal>]
let internal UpperMonthBoundary = 12uy

let private tryParseMonth (month: System.Byte) : Result<Month, DomainError> =    
    match month with
    | month when month >= LowerMonthBoundary &&
                 month <= UpperMonthBoundary -> Ok month
    | _ -> MonthOutOfRange |> Error

let tryParseFromInt32 (month: System.Int32) : Result<Month, DomainError> =
    try
        month
        |> System.Convert.ToByte
        |> tryParseMonth
    with _ -> MonthInvalidInt |> Error
        
let tryParseFromString (month: string) : Result<Month, DomainError> =
    match month |> System.Byte.TryParse with
    | true, month ->
        month
        |> tryParseMonth
    | _ -> MonthInvalidString |> Error