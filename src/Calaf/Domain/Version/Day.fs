module internal Calaf.Domain.Day

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal LowerDayBoundary= 1uy
[<Literal>]
let internal UpperDayBoundary = 31uy

let private tryParseDay (dayOfMonth: System.Byte) : Result<DayOfMonth, DomainError> =    
    match dayOfMonth with
    | dayOfMonth when dayOfMonth >= LowerDayBoundary &&
                      dayOfMonth <= UpperDayBoundary -> Ok dayOfMonth
    | _ -> DayOutOfRange |> Error

let tryParseFromInt32 (dayOfMonth: System.Int32) : Result<DayOfMonth, DomainError> =
    try
        dayOfMonth
        |> System.Convert.ToByte
        |> tryParseDay
    with _ -> DayInvalidInt |> Error