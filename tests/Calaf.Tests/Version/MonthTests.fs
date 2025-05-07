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
    let ``Leading zero out of range Year number string parses to OutOfRangeMonth error`` (leadingZeroOutOfRangeStringMonth: string) =
        tryParseFromString leadingZeroOutOfRangeStringMonth = (OutOfRangeMonth |> Error)
    
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to WrongStringMonth error`` (nonNumberStr: string) =        
        tryParseFromString nonNumberStr = (WrongStringMonth |> Error) 
    
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to WrongStringMonth error`` (badString: string) =
        tryParseFromString badString = (WrongStringMonth |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.wrongStringMonth> |], MaxTest = 200)>]
    let ``Wrong string parses to WrongStringYear error`` (wrongStringMonth: string) =
        wrongStringMonth |> tryParseFromString = (WrongStringMonth |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.outOfRangeStringMonth> |], MaxTest = 200)>]
    let ``String number out of 1-12 range parses to OutOfRangeMonth error`` (outOfRangeStringMonth: string) =
        tryParseFromString outOfRangeStringMonth = (OutOfRangeMonth |> Error)

module TryParseFromInt32PropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.inRangeByteMonth> |], MaxTest = 200)>]
    let ``Valid integer in-range parses to the Ok corresponding value`` (inRangeByteMonth: byte) =
        tryParseFromInt32 (int inRangeByteMonth) = Ok inRangeByteMonth
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Month.wrongInt32Month> |], MaxTest = 200)>]
    let ``Integer out of 1-12 range parses to OutOfRangeMonth error`` (wrongInt32Month: int) =
        tryParseFromInt32 wrongInt32Month = (WrongInt32Month |> Error)
        
module TryParseFromStringTests =
    [<Fact>]
    let ``Empty string returns WrongStringMonth error`` () =
        tryParseFromString "" = (WrongStringMonth |> Error)
        
    [<Fact>]
    let ``Whitespace returns None`` () =
        tryParseFromString " " = (WrongStringMonth |> Error)
        
    [<Fact>]
    let ``Zero return None`` () =
        tryParseFromString "0" = (WrongStringMonth |> Error)