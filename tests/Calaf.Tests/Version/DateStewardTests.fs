namespace Calaf.Tests.DateStewardTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DateSteward
open Calaf.Tests

module TryCreateMonthStampPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.DateSteward.inRangeDateTimeOffset> |], MaxTest = 200)>]
    let ``Any valid DateTime yields Ok with the corresponding value`` (inRangeDateTime: System.DateTimeOffset) =
        match tryCreateMonthStamp inRangeDateTime with
        | Ok dateStamp -> int dateStamp.Year = inRangeDateTime.Year &&
                          int dateStamp.Month = inRangeDateTime.Month
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.DateSteward.outOfRangeDateTimeOffset> |], MaxTest = 200)>]
    let ``Any DateTime with out of lower or upper year's boundaries yields YearOutOfRange error`` (outOfRangeDateTime: System.DateTimeOffset) =        
        tryCreateMonthStamp outOfRangeDateTime = (YearOutOfRange |> Error)
        
module TryCreateDayOfMonthPropertiesTests =
    [<Property>]
    let ``Any DateTimeOffset always represents a day in between the lower and upper boundaries`` (dateTimeOffset: System.DateTimeOffset) =
        let day = dateTimeOffset |> tryCreateDayOfMonth |> function 
            | Ok day  -> day
            | Error _ -> failwith "Expected a valid day, but got an error."
        test <@ day >= Day.lowerDayBoundary && day <= Day.upperDayBoundary @>