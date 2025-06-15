module internal Calaf.Domain.DateSteward

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

let tryCreateMonthStamp (bareDateTime: System.DateTimeOffset) : Result<MonthStamp, DomainError> =
    result {
        let! year  = Year.tryParseFromInt32 bareDateTime.Year
        let! month = Month.tryParseFromInt32 bareDateTime.Month
        return { Year = year; Month = month }        
    }
    
let tryCreateDayOfMonth (bareDateTime: System.DateTime) : Result<DayOfMonth, DomainError> =
    result {
        let! day = Day.tryParseFromInt32 bareDateTime.Day
        return day
    }