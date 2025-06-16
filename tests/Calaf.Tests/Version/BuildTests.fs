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
        //nightly.04.162+vnexsfsyhuzfedzkvtzslis - THIS STRING IS INVALID
        test <@ toString nightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
            
module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyString> |])>]
    let ``Nightly string recognizes correctly to the Nightly with number and hash`` (nightlyString: string) =
        //nightly.04.162+vnexsfsyhuzfedzkvtzslis - THIS STRING IS INVALID
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
    // TODO: Use generators instead
    [<Literal>]
    let private numberInitialValue =
        1us
    [<Literal>]
    let private dayIncreaseStep =
        1uy
    [<Literal>]
    let private numberIncreaseStep =
        1us
    let private bumpDay (day: byte) =
        if day = Day.upperDayBoundary
        then Day.lowerDayBoundary
        else day + dayIncreaseStep
    let private bumpNumber (number: uint16) =
        if number = System.UInt16.MaxValue
        then numberInitialValue
        else number + numberIncreaseStep
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuild> |])>]
    let ``Make new Nightly on a Nightly build returns a new one`` (nightlyBuild: Build) =
        match nightlyBuild with
        | Build.Nightly { Day = day; Number = number } ->
            let newBuildMetadata = {
                Day    = day    |> bumpDay
                Number = number |> bumpNumber
            }
            let build = Some nightlyBuild
            tryNightly build newBuildMetadata = Ok (Build.Nightly(newBuildMetadata))

    [<Property>]
    let ``Make Nightly on an empty Build returns Nightly`` () =
        let newNightlyBuild = { Day = 25uy; Number = 2us }
        let build = None
        tryNightly build newNightlyBuild = Ok (Build.Nightly(newNightlyBuild))
        
    [<Property>]
    let ``Make same Nightly on a Nightly build returns BuildAlreadyCurrent error`` () =
        let build = Some (Build.Nightly { Day = 05uy; Number = 1us })
        let newNightlyBuild = { Day = 05uy; Number = 1us }
        tryNightly build newNightlyBuild = Error BuildAlreadyCurrent