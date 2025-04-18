module internal Calaf.Domain.Year

open Calaf.Domain.DomainTypes

let private validate (year: System.UInt16) : Year option =
    let lowerBoundary = 1us
    let upperBoundary = System.UInt16.MaxValue
    match year with
    | year when year >= lowerBoundary &&
                year <= upperBoundary -> Some year
    | _ -> None
    
// TODO: Use ERROR instead of option
let tryParseFromInt32 (year: System.Int32) : Year option =
    try
        year
        |> System.Convert.ToUInt16
        |> validate
    with _ -> None

let tryParseFromString (year: string) : Year option =
    match year |> System.UInt16.TryParse with
    | true, year ->
        year |> validate
    | _ -> None