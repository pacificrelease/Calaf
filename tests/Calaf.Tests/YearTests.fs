namespace Calaf.Tests.YearTests

open FsCheck.Xunit

open Calaf.Tests
open Calaf.Domain.Year

module TryParseFromStringPropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.validYearUInt16> |], MaxTest = 200)>]
    let ``Valid string parses to corresponding value`` (validYear: uint16) =
        validYear
        |> string
        |> tryParseFromString = Some validYear
        
    [<Property(Arbitrary = [| typeof<Arbitrary.leadingZeroUInt16String> |], MaxTest = 200)>]
    let ``Leading zero year number string parses to corresponding value`` (validYear: string) =
        validYear
        |> tryParseFromString = Some(System.UInt16.Parse(validYear))
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to None`` (nonNumberStr: string) =
        nonNumberStr
        |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.badString> |], MaxTest = 200)>]
    let ``Bad string parses to None`` (badString: string) =
        badString
        |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowYearString> |], MaxTest = 200)>]
    let ``Number out of 1 - UInt16 max range parses to None`` (overflowYear: string) =
        overflowYear
        |> tryParseFromString = None
        
module TryParseFromInt32PropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.validYearUInt16> |], MaxTest = 200)>]
    let ``Valid integer (1 - UInt16 max) parses to the corresponding value`` (validYear: uint16) =
        validYear
        |> int
        |> tryParseFromInt32 = Some(uint16 validYear)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowYearInt32> |], MaxTest = 200)>]
    let ``Integer out of valid range (1 - UInt16 max) parses to None`` (overflowYear: int) =
        overflowYear
        |> tryParseFromInt32 = None