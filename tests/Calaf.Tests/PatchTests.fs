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
        nonNumberStr
        |> tryParseFromString= None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowPatchString> |], MaxTest = 200)>]
    let ``Number out of 1 - UInt32 max range parses to None`` (overflowPatch: string) =
        overflowPatch
        |> tryParseFromString = None
        
module BumpPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Bump Patch with value less than uint32 - 1 increments value by one`` (patch: Calaf.Domain.DomainTypes.Patch) =        
        patch
         |> Some
         |> bump = patch + 1u
         
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Bump Patch always returns a positive value`` (patch: Calaf.Domain.DomainTypes.Patch option) =
        patch
        |> bump > 0u
        
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Bump Patch always preserves ordering`` (patch: Calaf.Domain.DomainTypes.Patch) =
        let bumped1 = patch   |> Some |> bump
        let bumped2 = bumped1 |> Some |> bump
        let bumped3 = bumped2 |> Some |> bump
        bumped2 > bumped1 && bumped3 > bumped2
        
module BumpTests =
    [<Fact>]
    let ``Maximum uint32 bumps to 1`` () =
        System.UInt32.MaxValue
        |> Some
        |> bump = 1u
        
    [<Fact>]
    let ``None bumps to 1`` () =
        None
        |> bump = 1u
        
    [<Fact>]
    let ``Maximum uint32 - 1 bumps to Maximum uint32 - 1`` () =
        System.UInt32.MaxValue - 1u
        |> Some
        |> bump = System.UInt32.MaxValue    
    