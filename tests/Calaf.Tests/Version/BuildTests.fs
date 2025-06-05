namespace Calaf.Tests.BuildTests

open FsCheck.Xunit

open Calaf.Domain
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Build
open Calaf.Tests

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyString> |])>]
    let ``Nightly string recognizes correctly to the corresponding value`` (nightlyString: string) =
        nightlyString
        |> tryParseFromString = Ok (Some Build.Nightly)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Null or empty or whitespace string parses to the None`` (badString: string) =
        badString
        |> tryParseFromString = Ok None
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.wrongString> |])>]
    let ``Arbitrary string parses to BuildInvalidString error`` (wrongStringBuild: string) =
        wrongStringBuild
        |> tryParseFromString = Error BuildInvalidString
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.containingNightlyBadString> |])>]
    let ``String containing Nightly parses to BuildInvalidString error`` (containingNightlyBadString: string) =
        containingNightlyBadString
        |> tryParseFromString = Error BuildInvalidString
        
module BumpPropertyTests =
    [<Property>]
    let ``Bumping a Nightly build does not change it`` () =
        let build = Some Build.Nightly
        bump build = Build.Nightly
        
    [<Property>]
    let ``Bump empty Build returns Nightly`` () =
        let build = None
        bump build = Build.Nightly