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
            
    let internal uniqueDateTimeOffset (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =        
        let incrStep = 1
        
        let newMonth (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddMonths(incrStep).Month
            
        let newYear (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddYears(incrStep).Year
            
        match (v.Year <> uint16 dateTimeOffset.Year, v.Month <> byte dateTimeOffset.Month) with
        | true, true ->
            dateTimeOffset
        | true, false ->
            let year = newYear dateTimeOffset
            (year, int v.Month, dateTimeOffset.Day)
            |> System.DateTime
            |> System.DateTimeOffset
        | false, true ->
            let month = newMonth dateTimeOffset
            (int v.Year, month, dateTimeOffset.Day)
            |> System.DateTime
            |> System.DateTimeOffset
        | false, false ->
            let year = newYear dateTimeOffset
            let month = newMonth dateTimeOffset
            (int year, month, dateTimeOffset.Day)
            |> System.DateTime
            |> System.DateTimeOffset
            
    let internal asDateTimeOffset (calVer: CalendarVersion) =
        (int calVer.Year, int calVer.Month, 1) |> System.DateTime |> System.DateTimeOffset