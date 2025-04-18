namespace Calaf.Tests.YearTests

open FsCheck.Xunit

open Calaf.Tests
open Calaf.Domain.Year

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validYearUInt16> |])>]
    let ``Valid string parses to corresponding value`` (validYear: uint16) =
        validYear
        |> string
        |> tryParseFromString = Some validYear
        
    [<Property(Arbitrary = [| typeof<Arbitrary.leadingZeroUInt16String> |])>]
    let ``Leading zero year number string parses to corresponding value`` (validYear: string) =
        validYear
        |> tryParseFromString = Some(System.UInt16.Parse(validYear))
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parses to None`` (nonNumberStr: string) =
        nonNumberStr
        |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowYearString> |])>]
    let ``Number out of 1 - UInt16 max range parses to None`` (overflowYear: string) =
        overflowYear
        |> tryParseFromString = None