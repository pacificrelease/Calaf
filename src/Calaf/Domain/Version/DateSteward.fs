module internal Calaf.Domain.DateSteward

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

let tryCreate (bareDateTime: System.DateTime) : Result<MonthStamp, DomainError> =
    result {
        let! year  = Year.tryParseFromInt32 bareDateTime.Year
        let! month = Month.tryParseFromInt32 bareDateTime.Month
        return { Year = year; Month = month }        
    }
