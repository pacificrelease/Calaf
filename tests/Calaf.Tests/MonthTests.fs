namespace Calaf.Tests.MonthTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain.Month
open Calaf.Tests

module TryParseFromStringPropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.validMonthByte> |])>]
    let ``Valid string parses to corresponding value`` (validMonth: byte) =
        validMonth
        |> string
        |> tryParseFromString = Some(byte validMonth)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.leadingZeroDigitString> |])>]
    let ``Leading zero month digit string parses to corresponding value`` (validMonth: string) =
        validMonth
        |> tryParseFromString = Some(System.Byte.Parse(validMonth))
    
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parses to None`` (nonNumberStr: string) =
        nonNumberStr
        |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowMonthString> |])>]
    let ``Number out of 1-12 range parses to None`` (overflowMonth: string) =
        overflowMonth
        |> tryParseFromString = None

module TryParseFromInt32PropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validMonthByte> |])>]
    let ``Valid integer parses to corresponding value`` (validMonth: byte) =
        validMonth
        |> int
        |> tryParseFromInt32 = Some(byte validMonth)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowMonthInt32> |])>]
    let ``Integers out of 1-12 range parses to None`` (overflowMonth: int) =
        overflowMonth
        |> tryParseFromInt32 = None
        
module TryParseFromStringTests =
    [<Fact>]
    let ``Empty string returns None`` () =
        tryParseFromString "" = None
        
    [<Fact>]
    let ``Whitespace returns None`` () =
        tryParseFromString " " = None
        
    [<Fact>]
    let ``Zero return None`` () =
        tryParseFromString "0" = None