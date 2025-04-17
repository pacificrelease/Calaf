module internal Calaf.Domain.Year

open Calaf.Domain.DomainTypes

let private validate (lowerBoundary: uint16, upperBoundary: uint16) (year: System.UInt16) : Year option =
    match year with
    | year when year >= lowerBoundary &&
                year <= upperBoundary -> Some year
    | _ -> None    

let tryParseFromString (year: string) : Year option =
    let validate = validate (1900us, 40000us)
    match year |> System.UInt16.TryParse with
    | true, year ->
        year |> validate
    | _ -> None