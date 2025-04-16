namespace Calaf.Tests.PatchTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain.Patch
open Calaf.Tests

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroUInt32> |])>]
    let ``Valid uint32 greater than zero string parse to corresponding value`` (validPatch: uint32) =
        validPatch
        |> string
        |> tryParseFromString  = Some validPatch
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parse to None`` (nonNumberPatchStr: string) =
        nonNumberPatchStr
        |> string
        |> tryParseFromString= None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowUInt32String> |])>]
    let ``Number out of uint32 range parse to None`` (overflowPatch: string) =
        overflowPatch
        |> tryParseFromString= None
        
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