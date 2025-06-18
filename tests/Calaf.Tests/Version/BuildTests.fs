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

module NightlyTests =
    // Result is always Build.Nightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuildOption>; typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly always returns a Nightly build type`` (build: Build option, dayOfMonth: DayOfMonth) =
        let nightly = nightly build dayOfMonth
        test <@ match nightly with | Build.Nightly _ -> true @>
        
    // Day is correctly assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuildOption>; typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``The day of the month is correctly assigned to the Nightly build`` (nightlyBuild: Build option, dayOfMonth: DayOfMonth) =
        let nightlyBuild' = nightly nightlyBuild dayOfMonth
        let dayOfMonth' = match nightlyBuild' with | Build.Nightly { Day = day } -> day
        test <@ dayOfMonth' = dayOfMonth @>
        
    // Number starts at NumberStartValue if no current build
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly build starts at NumberStartValue if no current build`` (dayOfMonth: DayOfMonth) =
        let nightlyBuild = nightly None dayOfMonth
        let buildNumber = match nightlyBuild with | Build.Nightly { Number = number } -> number
        test <@ buildNumber = NumberStartValue @>
        
    //Number increments for the same day build with no overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.numberNoUpperBoundaryNightlyBuild> |])>]
    let ``Nightly build with no upper boundary number increments the number for the same day nightly build`` (nightlyBuild: Build) =
        let dayOfMonth = match nightlyBuild with | Build.Nightly { Day = day } -> day
        let nightlyBuild' = nightly (Some nightlyBuild) dayOfMonth
        let number  = match nightlyBuild  with | Build.Nightly { Number = number } -> number
        let number' = match nightlyBuild' with | Build.Nightly { Number = number } -> number
        let difference = number' - number        
        test <@ difference = NumberIncrementStep @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly build resets the number to NumberStartValue on overflow`` (dayOfMonth: DayOfMonth) =
        let nightlyBuild = Build.Nightly { Day = dayOfMonth; Number = BuildNumber.MaxValue } |> Some                
        let nightlyBuild' = nightly nightlyBuild dayOfMonth
        let number' = match nightlyBuild' with | Build.Nightly { Number = number } -> number
        test <@ number' = NumberStartValue @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.nightlyBuild>; typeof<Arbitrary.Day.inRangeByteDay> |])>]
    let ``Nightly build with different day resets the number to NumberStartValue`` (nightlyBuild: Build, dayOfMonth: DayOfMonth) =
        let currentDay = match nightlyBuild with | Build.Nightly { Day = day } -> day
        let currentDay =
            if currentDay <> dayOfMonth
            then dayOfMonth
            else Day.LowerDayBoundary
        let nightlyBuild' = nightly (Some nightlyBuild) currentDay
        let number' = match nightlyBuild' with | Build.Nightly { Number = number } -> number
        
        test <@ number' = NumberStartValue @>