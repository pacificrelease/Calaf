namespace Calaf.Tests.BuildTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Build
open Calaf.Tests

module ToStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuild> |])>]
    let ``Nightly build converts to non-empty string`` (nightlyBuild: Build) =       
        test <@ toString nightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
            
module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyString> |])>]
    let ``Nightly string recognizes correctly to the Nightly with number and hash`` (nightlyString: string) =
        test <@
            let result = nightlyString |> tryParseFromString
            match result with
            | Ok (Some (Build.Nightly _)) -> true
            | _ -> false @>
        
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
        let result = containingNightlyBadString |> tryParseFromString
        result = Error BuildInvalidString
    
// TODO: Update tests to use generators
module NightlyPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuild> |])>]
    let ``Make new Nightly on a Nightly build with hash returns a new Nightly`` (nightlyBuild: Build) =
        match nightlyBuild with
        | Build.Nightly { Number = number; Hash = hash } ->
            let newBuildMetadata = { Number = number + 1uy; Hash = hash |> Option.map (fun h -> h.ToCharArray() |> Array.rev |> System.String) }
            let build = Some nightlyBuild
            tryNightly build newBuildMetadata = Ok (Build.Nightly(newBuildMetadata))

    [<Property>]
    let ``Make Nightly on an empty Build returns Nightly`` () =
        let newBuildMetadata = { Number = 2uy; Hash = Some "hash2" }
        let build = None
        tryNightly build newBuildMetadata = Ok (Build.Nightly(newBuildMetadata))
        
    [<Property>]
    let ``Make same Nightly on a Nightly build returns BuildAlreadyCurrent error`` () =
        let build = Some (Build.Nightly { Number = 1uy; Hash = Some "hash" })
        let newBuildMetadata = { Number = 1uy; Hash = Some "hash" }
        tryNightly build newBuildMetadata = Error BuildAlreadyCurrent