namespace Calaf.Tests.MonthTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain
open Calaf.Domain.Month
open Calaf.Tests

module TryParseFromStringPropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.inRangeByteMonth> |], MaxTest = 200 )>]
    let ``Valid string parses to Ok corresponding value`` (inRangeByteMonth: byte) =
        tryParseFromString (string inRangeByteMonth) = Ok inRangeByteMonth
        
    // TODO: make leadingZeroInRangeStringMonth
    // TODO: make leadingZeroWrongStringMonth
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.leadingZeroOutOfRangeStringMonth> |], MaxTest = 200)>]
    let ``Leading zero out of range Year number string parses to MonthOutOfRange error`` (leadingZeroOutOfRangeStringMonth: string) =
        tryParseFromString leadingZeroOutOfRangeStringMonth = (MonthOutOfRange |> Error)
    
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to MonthInvalidString error`` (nonNumberStr: string) =        
        tryParseFromString nonNumberStr = (MonthInvalidString |> Error) 
    
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to MonthInvalidString error`` (badString: string) =
        tryParseFromString badString = (MonthInvalidString |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.wrongStringMonth> |], MaxTest = 200)>]
    let ``Wrong string parses to MonthInvalidString error`` (wrongStringMonth: string) =
        wrongStringMonth |> tryParseFromString = (MonthInvalidString |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.outOfRangeStringMonth> |], MaxTest = 200)>]
    let ``String number out of 1-12 range parses to MonthOutOfRange error`` (outOfRangeStringMonth: string) =
        tryParseFromString outOfRangeStringMonth = (MonthOutOfRange |> Error)

module TryParseFromInt32PropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.inRangeByteMonth> |], MaxTest = 200)>]
    let ``Valid integer in-range parses to the Ok corresponding value`` (inRangeByteMonth: byte) =
        tryParseFromInt32 (int inRangeByteMonth) = Ok inRangeByteMonth
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.wrongInt32Month> |], MaxTest = 200)>]
    let ``Integer out of 1-12 range parses to MonthInvalidInt error`` (wrongInt32Month: int) =
        tryParseFromInt32 wrongInt32Month = (MonthInvalidInt |> Error)
        
module TryParseFromStringTests =
    [<Fact>]
    let ``Empty string returns MonthInvalidString error`` () =
        tryParseFromString "" = (MonthInvalidString |> Error)
        
    [<Fact>]
    let ``Whitespace returns MonthInvalidString error`` () =
        tryParseFromString " " = (MonthInvalidString |> Error)
        
    [<Fact>]
    let ``Zero return MonthInvalidString error`` () =
        tryParseFromString "0" = (MonthInvalidString |> Error)