namespace Calaf.Tests.PatchTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain.Patch
open Calaf.Tests

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validPatchUInt32> |], MaxTest = 200)>]
    let ``Valid string parses to the Some + corresponding value`` (validPatch: uint32) =
        validPatch
        |> string
        |> tryParseFromString  = Some validPatch
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to None`` (nonNumberStr: string) =
        nonNumberStr |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to None`` (badStr: string) =
        badStr |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowPatchString> |], MaxTest = 200)>]
    let ``Number out of 1 - UInt32 max range parses to None`` (overflowPatch: string) =
        overflowPatch
        |> tryParseFromString = None
        
module ReleasePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Patch with value less than uint32 - 1 increments value by one`` (patch: Calaf.Domain.DomainTypes.Values.Patch) =        
        patch
         |> Some
         |> release = patch + 1u
         
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Patch always returns a positive value`` (patch: Calaf.Domain.DomainTypes.Values.Patch option) =
        patch
        |> release > 0u
        
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Patch always preserves ordering`` (patch: Calaf.Domain.DomainTypes.Values.Patch) =
        let release   = patch    |> Some |> Calaf.Domain.Patch.release
        let release'  = release  |> Some |> Calaf.Domain.Patch.release
        let release'' = release' |> Some |> Calaf.Domain.Patch.release
        release' > release && release'' > release'
        
module BumpTests =
    [<Fact>]
    let ``Maximum uint32 release to 1`` () =
        System.UInt32.MaxValue
        |> Some
        |> release = 1u
        
    [<Fact>]
    let ``None release to 1`` () =
        None
        |> release = 1u
        
    [<Fact>]
    let ``Maximum uint32 - 1 release to Maximum uint32 - 1`` () =
        System.UInt32.MaxValue - 1u
        |> Some
        |> release = System.UInt32.MaxValue    
    