module internal Calaf.Domain.Year

open Calaf.Domain.Errors
open Calaf.Domain.DomainTypes

[<Literal>]
let lowerYearBoundary = 1970us
[<Literal>]
let upperYearBoundary = 9999us

let private tryParseYear (year: System.UInt16) : Result<Year, CalafError> =    
    match year with
    | year when year >= lowerYearBoundary &&
                year <= upperYearBoundary -> Ok year
    | _ -> OutOfRangeYear |> Validation |> Error

let tryParseFromInt32 (year: System.Int32) : Result<Year, CalafError> =
    try
        year
        |> System.Convert.ToUInt16
        |> tryParseYear
    with _ -> WrongInt32Year |> Validation |> Error

let tryParseFromString (year: string) : Result<Year, CalafError> =
    match year |> System.UInt16.TryParse with
    | true, year -> year |> tryParseYear
    | _ -> WrongStringYear |> Validation |> Error