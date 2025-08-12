namespace Calaf.Tests.MicroTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain.Micro
open Calaf.Tests

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validMicroUInt32> |], MaxTest = 200)>]
    let ``Valid string parses to the Some + corresponding value`` (validMicro: uint32) =
        validMicro
        |> string
        |> tryParseFromString  = Some validMicro
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to None`` (nonNumberStr: string) =
        nonNumberStr |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to None`` (badStr: string) =
        badStr |> tryParseFromString = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.overflowMicroString> |], MaxTest = 200)>]
    let ``Number out of 1 - UInt32 max range parses to None`` (overflowMicro: string) =
        overflowMicro
        |> tryParseFromString = None
        
module ReleasePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Micro with value less than uint32 - 1 increments value by one`` (micro: Calaf.Domain.DomainTypes.Values.Micro) =        
        micro
         |> Some
         |> release = micro + 1u
         
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Micro always returns a positive value`` (micro: Calaf.Domain.DomainTypes.Values.Micro option) =
        micro
        |> release > 0u
        
    [<Property(Arbitrary = [| typeof<Arbitrary.greaterThanZeroBeforeUInt32MinusOne> |], MaxTest = 200)>]
    let ``Release Micro always preserves ordering`` (micro: Calaf.Domain.DomainTypes.Values.Micro) =
        let release   = micro    |> Some |> Calaf.Domain.Micro.release
        let release'  = release  |> Some |> Calaf.Domain.Micro.release
        let release'' = release' |> Some |> Calaf.Domain.Micro.release
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