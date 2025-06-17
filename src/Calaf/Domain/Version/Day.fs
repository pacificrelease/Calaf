module internal Calaf.Domain.Day

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let lowerDayBoundary= 1uy
[<Literal>]
let upperDayBoundary = 31uy

let private tryParseDay (dayOfMonth: System.Byte) : Result<DayOfMonth, DomainError> =    
    match dayOfMonth with
    | dayOfMonth when dayOfMonth >= lowerDayBoundary &&
                      dayOfMonth <= upperDayBoundary -> Ok dayOfMonth
    | _ -> DayOutOfRange |> Error

let tryParseFromInt32 (dayOfMonth: System.Int32) : Result<DayOfMonth, DomainError> =
    try
        dayOfMonth
        |> System.Convert.ToByte
        |> tryParseDay
    with _ -> DayInvalidInt |> Error