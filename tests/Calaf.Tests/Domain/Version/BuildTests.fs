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

module NightlyTests =
    // Result is always Build.Nightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuildOption>; typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly always returns a Nightly build type`` (build: Build option, dayOfMonth: DayOfMonth) =
        let nightly = nightly build dayOfMonth
        test <@ match nightly with | Build.Nightly _ -> true | _ -> false @>
        
    // Day is correctly assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuildOption>; typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``The day of the month is correctly assigned to the Nightly build`` (nightlyBuild: Build option, dayOfMonth: DayOfMonth) =        
        let nightlyBuild' = nightly nightlyBuild dayOfMonth
        match nightlyBuild' with
        | Build.Nightly { Day = actualDay } ->
            test <@ actualDay = dayOfMonth @>
        | _ -> test <@ false @>
        
    // Number starts at NumberStartValue if no current build
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly build starts at NumberStartValue if no current build`` (dayOfMonth: DayOfMonth) =
        let nightlyBuild = nightly None dayOfMonth
        match nightlyBuild with
        | Build.Nightly { Number = number } ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    //Number increments for the same day build with no overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.numberNoUpperBoundaryNightlyBuild> |])>]
    let ``Nightly build with no upper boundary number increments the number for the same day nightly build`` (nightlyBuild: Build) =
        match nightlyBuild with
        | Build.Nightly { Day = dayOfMonth; Number = number } ->
            let nightlyBuild' = nightly (Some nightlyBuild) dayOfMonth            
            match nightlyBuild' with
            | Build.Nightly { Number = number' } ->
                test <@ number' - number = NumberIncrementStep @>
            | _ -> test <@ false  @>
        | _ -> test <@ false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly build resets the number to NumberStartValue on overflow`` (dayOfMonth: DayOfMonth) =
        let nightlyBuild = Build.Nightly { Day = dayOfMonth; Number = BuildNumber.MaxValue } |> Some                
        let nightlyBuild' = nightly nightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Build.Nightly { Number = number } ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuild> |])>]
    let ``Nightly build with different day resets the number to NumberStartValue``
        (nightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match nightlyBuild with
        | Build.Nightly { Day = dayOfMonth; Number = _ } ->
            let differentDay =
                if dayOfMonth = byte dateTimeOffset.Day
                then dateTimeOffset.AddDays(1).Day |> byte
                else byte dateTimeOffset.Day
            let nightlyBuild' = nightly (Some nightlyBuild) differentDay
            match nightlyBuild' with
            | Build.Nightly { Number = number' } ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>