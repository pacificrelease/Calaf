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
            
    let internal uniqueDateTimeOffset (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =        
        let incrStep = 1
        
        let newMonth (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddMonths(incrStep)
            |> System.DateTimeOffset
            
        let newYear (dateTimeOffset: System.DateTimeOffset) =
            dateTimeOffset.Date.AddYears(incrStep)
            |> System.DateTimeOffset
        
        let skipYear, skipMonth = (v.Year <> uint16 dateTimeOffset.Year, v.Month <> byte dateTimeOffset.Month)
        
        match skipYear, skipMonth with
        | true, true ->
            dateTimeOffset
        | true, false ->
            newMonth dateTimeOffset
        | false, true ->
            newYear dateTimeOffset 
        | false, false ->
            let dt = newYear dateTimeOffset
            newMonth dt
            
    let internal asDateTimeOffset (calVer: CalendarVersion) =
        (int calVer.Year, int calVer.Month, 1) |> System.DateTime |> System.DateTimeOffset