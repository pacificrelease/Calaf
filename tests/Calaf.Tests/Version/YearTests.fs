namespace Calaf.Tests.YearTests

open FsCheck.Xunit

open Calaf.Domain
open Calaf.Domain.Year
open Calaf.Tests

module TryParseFromStringPropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.inRangeUInt16Year> |], MaxTest = 200)>]
    let ``Valid string parses to Ok corresponding value`` (validYear: uint16) =
        validYear |> string |> tryParseFromString = Ok validYear
    
    // TODO: make leadingZeroInRangeStringYear
    // TODO: make leadingZeroWrongStringYear
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.leadingZeroOutOfRangeStringYear> |], MaxTest = 200)>]
    let ``Leading zero out of range Year number string parses to YearOutOfRange error`` (outOfRangeYearString: string) =
        outOfRangeYearString |> tryParseFromString = (YearOutOfRange |> Error)
        
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to YearInvalidString error`` (nonNumberStr: string) =
        nonNumberStr |> tryParseFromString = (YearInvalidString |> Error)
       
    // TODO: Remove because of duplication with ``Wrong string parses to WrongStringYear error``
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to YearInvalidString error`` (badString: string) =
        badString |> tryParseFromString = (YearInvalidString |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.wrongStringYear> |], MaxTest = 200)>]
    let ``Wrong string parses to YearInvalidString error`` (wrongStringYear: string) =
        wrongStringYear |> tryParseFromString = (YearInvalidString |> Error)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.outOfRangeStringYear> |], MaxTest = 200)>]
    let ``Number out of lower and upper boundaries range parses to YearOutOfRange error`` (outOfRangeStringYear: string) =
        outOfRangeStringYear |> tryParseFromString = (YearOutOfRange |> Error)
        
module TryParseFromInt32PropertyTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.inRangeUInt16Year> |], MaxTest = 200)>]
    let ``Valid integer in-range parses to the Ok corresponding value`` (inRangeUInt16Year: uint16) =        
        tryParseFromInt32 (int inRangeUInt16Year) = Ok inRangeUInt16Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Year.wrongInt32Year> |], MaxTest = 200)>]
    let ``Integer out of valid range (1 - UInt16 max) parses to YearInvalidInt error`` (wrongInt32Year: int) =
        wrongInt32Year |> tryParseFromInt32 = (YearInvalidInt |> Error)