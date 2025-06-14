namespace Calaf.Tests.DayTests

open FsCheck.Xunit

open Calaf.Domain
open Calaf.Domain.Day
open Calaf.Tests

module TryParseFromInt32PropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.inRangeByteDay> |], MaxTest = 200)>]
    let ``Valid integer in-range parses to the Ok corresponding value`` (inRangeByteDay: byte) =
        tryParseFromInt32 (int inRangeByteDay) = Ok inRangeByteDay
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.outOfRangeByteDay> |], MaxTest = 200)>]
    let ``Integer out of 1-31 range parses to DayOutOfRange error`` (outOfRangeByteDay: int) =
        tryParseFromInt32 outOfRangeByteDay = (DayOutOfRange |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.wrongInt32Day> |], MaxTest = 200)>]
    let ``Integer out of 1-31 range parses to DayInvalidInt error`` (wrongInt32Day: int) =
        tryParseFromInt32 wrongInt32Day = (DayInvalidInt |> Error)