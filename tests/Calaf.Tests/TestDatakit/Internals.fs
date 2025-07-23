namespace Calaf.Tests

module internal Internals =
    open Calaf.Domain.DomainTypes.Values
    
    let internal preventPatchOverflow (patch: Patch option) =
        match patch with
        | Some p ->            
            if p = Patch.MaxValue then
                p - Calaf.Domain.Patch.PatchIncrementStep |> Some
            else Some p
        | None -> None
    
    let internal preventNumberOverflow (number: BuildNumber) =
        if number = BuildNumber.MaxValue then
            number - Calaf.Domain.Build.NumberIncrementStep
        else number
    
    let internal uniqueDay (dayOfMonth: DayOfMonth, dateTimeOffset: System.DateTimeOffset) =
        if dayOfMonth = byte dateTimeOffset.Day then
            byte (dateTimeOffset.AddDays(1).Day)
        else byte dateTimeOffset.Day
    
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