module internal Calaf.Domain.Year

open Calaf.Domain.DomainTypes

[<Literal>]
let lowerYearBoundary = 1970us
[<Literal>]
let upperYearBoundary = 9999us

let private tryParseYear (year: System.UInt16) : Year option =    
    match year with
    | year when year >= lowerYearBoundary &&
                year <= upperYearBoundary -> Some year
    | _ -> None
    
// TODO: Use ERROR instead of option
let tryParseFromInt32 (year: System.Int32) : Year option =
    try
        year
        |> System.Convert.ToUInt16
        |> tryParseYear
    with _ -> None

let tryParseFromString (year: string) : Year option =
    match year |> System.UInt16.TryParse with
    | true, year -> year |> tryParseYear
    | _ -> None