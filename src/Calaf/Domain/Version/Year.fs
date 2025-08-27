module internal Calaf.Domain.Year

open Calaf.Domain.DomainTypes

[<Literal>]
let internal LowerYearBoundary = 1us
[<Literal>]
let internal UpperYearBoundary = 9999us

let private tryParseYear (year: System.UInt16) : Result<Year, DomainError> =    
    match year with
    | year when year >= LowerYearBoundary &&
                year <= UpperYearBoundary -> Ok year
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