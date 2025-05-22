module internal Calaf.Domain.Year

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let lowerYearBoundary = 1970us
[<Literal>]
let upperYearBoundary = 9999us

let private tryParseYear (year: System.UInt16) : Result<Year, DomainError> =    
    match year with
    | year when year >= lowerYearBoundary &&
                year <= upperYearBoundary -> Ok year
    | _ -> YearOutOfRange |> Error

let tryParseFromInt32 (year: System.Int32) : Result<Year, DomainError> =
    try
        year
        |> System.Convert.ToUInt16
        |> tryParseYear
    with _ -> YearInvalidInt |> Error

let tryParseFromString (year: string) : Result<Year, DomainError> =
    match year |> System.UInt16.TryParse with
    | true, year -> year |> tryParseYear
    | _ -> YearInvalidString |> Error