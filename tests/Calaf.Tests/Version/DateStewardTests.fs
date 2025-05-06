namespace Calaf.Tests.DateStewardTests

open FsCheck.Xunit

open Calaf.Domain.DomainErrors
open Calaf.Domain.DateSteward
open Calaf.Tests

module TryCreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.DateSteward.inRangeDateTime> |], MaxTest = 200)>]
    let ``Any valid DateStamp yields Ok with the corresponding value`` (inRangeDateTime: System.DateTime) =
        match tryCreate inRangeDateTime with
        | Ok dateStamp -> int dateStamp.Year = inRangeDateTime.Year &&
                          int dateStamp.Month = inRangeDateTime.Month
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.DateSteward.outOfRangeDateTime> |], MaxTest = 200)>]
    let ``Any DateTime with out of lower or upper boundaries range year yields OutOfRangeYear error`` (outOfRangeDateTime: System.DateTime) =        
        tryCreate outOfRangeDateTime = (OutOfRangeYear |> Error)