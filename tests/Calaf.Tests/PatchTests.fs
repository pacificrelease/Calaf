module Calaf.Tests.PatchTests

open FsCheck.Xunit

open Calaf.Domain.Patch
open Calaf.Tests

module TryParseFromStringPropertyTests =
    [<Property>]
    let ``Valid uint32 string parse to corresponding value`` (validPatch: uint32) =
        validPatch
        |> string
        |> tryParseFromString  = Some validPatch
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parse to None`` (nonNumberPatchStr: string) =
        nonNumberPatchStr
        |> string
        |> tryParseFromString= None