namespace Calaf.Tests

module internal Internals =
    open Calaf.Domain.DomainTypes.Values
    
    let internal uniqueMonthStamp (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =        
        let incrStep = 1
        
        let newMonth (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddMonths(incrStep).Month
            
        let newYear (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddYears(incrStep).Year
            
        match (calVer.Year <> uint16 dateTimeOffset.Year, calVer.Month <> byte dateTimeOffset.Month) with
        | true, true ->
            { Year = dateTimeOffset.Year |> uint16
              Month = dateTimeOffset.Month |> byte }
        | true, false ->
            { Year = dateTimeOffset.Year |> uint16
              Month = newMonth dateTimeOffset |> byte }
        | false, true ->
            { Year = newYear dateTimeOffset |> uint16
              Month = dateTimeOffset.Month |> byte }
        | false, false ->
            { Year = newYear dateTimeOffset |> uint16
              Month = newMonth dateTimeOffset |> byte }