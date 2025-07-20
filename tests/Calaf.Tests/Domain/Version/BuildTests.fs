namespace Calaf.Tests.BuildTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Build
open Calaf.Tests

module ToStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build converts to non-empty string`` (betaBuild: Build) =
        test <@ toString betaBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build string starts with the right build type prefix`` (betaBuild: Build) =
        let betaBuildString = toString betaBuild
        test <@ betaBuildString.StartsWith BetaBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build converts to non-empty string`` (nightlyBuild: Build) =
        test <@ toString nightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build string starts with the right build type prefix`` (nightlyBuild: Build) =
        let nightlyBuildString = toString nightlyBuild
        test <@ nightlyBuildString.StartsWith NightlyBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build converts to non-empty string`` (betaNightlyBuild: Build) =
        test <@ toString betaNightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build string starts with the right build type prefix`` (betaNightlyBuild: Build) =
        let betaNightlyBuildString = toString betaNightlyBuild
        test <@ betaNightlyBuildString.StartsWith BetaBuildType @>
            
module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaString> |])>]
    let ``Beta string recognizes correctly to the Beta with number`` (betaString: string) =
        let result = betaString |> tryParseFromString
        test <@ match result with | Ok (Some (Build.Beta _)) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyString> |])>]
    let ``Nightly string recognizes correctly to the Nightly with day and number`` (nightlyString: string) =
        let result = nightlyString |> tryParseFromString
        test <@ match result with | Ok (Some (Build.Nightly _)) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyString> |])>]
    let ``BetaNightly string recognizes correctly to the Beta with number`` (betaNightlyString: string) =
        let result = betaNightlyString |> tryParseFromString
        test <@ match result with | Ok (Some (Build.BetaNightly _)) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Null or empty or whitespace string parses to the None`` (nullOrWhiteSpaceString: string) =
        let build = tryParseFromString nullOrWhiteSpaceString
        test <@ build = Ok None @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.wrongString> |])>]
    let ``Arbitrary string parses to BuildInvalidString error`` (wrongStringBuild: string) =
        let build = tryParseFromString wrongStringBuild
        test <@ build = Error BuildInvalidString @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.containingBetaBadString> |])>]
    let ``String containing Beta parses to BuildInvalidString error`` (containingBetaBadString: string) =
        let build = containingBetaBadString |> tryParseFromString
        test <@ build = Error BuildInvalidString @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.containingNightlyBadString> |])>]
    let ``String containing Nightly parses to BuildInvalidString error`` (containingNightlyBadString: string) =
        let result = containingNightlyBadString |> tryParseFromString
        result = Error BuildInvalidString
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.containingBetaNightlyBadString> |])>]
    let ``String containing BetaNightly parses to BuildInvalidString error`` (containingBetaNightlyBadString: string) =
        let build = containingBetaNightlyBadString |> tryParseFromString
        test <@ build = Error BuildInvalidString @>

module BetaTests =
    // Result is always Build.Beta
    [<Property>]
    let ``None build always releases a Beta build type`` () =
        let beta = beta None
        test <@ beta.IsBeta @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta always releases a Beta build type`` (build: Build) =
        let beta = beta (Some build)
        test <@ beta.IsBeta @>
        
    // Number is greater then assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The number is increased for Beta build`` (b: BetaBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = beta (Some (Build.Beta b))        
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' > b.Number @>
        | _ -> test <@ false @>
    
    // Number increased on the step value     
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The number is increased on step value for Beta build`` (b: BetaBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = beta (Some (Build.Beta b))        
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' - b.Number = NumberIncrementStep @>
        | _ -> test <@ false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The Beta number resets the number to NumberStartValue on overflow`` (b: BetaBuild) =
        let b = { b with Number = BuildNumber.MaxValue }
        let build' = beta (Some (Build.Beta b))        
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly always releases a Beta build type`` (build: Build) =
        let beta = beta (Some build)
        test <@ beta.IsBeta @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``Nightly always releases a Beta build with NumberStartValue`` (n: NightlyBuild) =
        let build' = beta (Some (Build.Nightly n))   
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly always releases a Beta build type`` (build: Build) =
        let beta = beta (Some build)
        test <@ beta.IsBeta @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly always increases number of Beta build type when releases beta`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = beta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' > b.Number @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly always increases number by step when releases beta`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = beta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' - b.Number = NumberIncrementStep @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly resets the number to NumberStartValue on overflow`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = BuildNumber.MaxValue }
        let build' = beta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Build.Beta { Number = number' } ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
module NightlyTests =
    // Result is always Build.Nightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuildOption> |])>]
    let ``Nightly build type always returns a Nightly build type when releases nightly``
        (build: Build option, dateTimeOffset: System.DateTimeOffset) =
        let nightly = nightly build (byte dateTimeOffset.Day)
        test <@ nightly.IsNightly @>
        
    // Day is correctly assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuildOption> |])>]
    let ``The day of the month is correctly assigned to the Nightly build`` (nightlyBuild: Build option, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let nightlyBuild' = nightly nightlyBuild dayOfMonth
        match nightlyBuild' with
        | Build.Nightly { Day = actualDay } ->
            test <@ actualDay = dayOfMonth @>
        | _ -> test <@ false @>
        
    // Number starts at NumberStartValue if no current build
    [<Property>]
    let ``Nightly build starts at NumberStartValue if no current build`` (dateTimeOffset: System.DateTimeOffset) =
        let nightlyBuild = nightly None (byte dateTimeOffset.Day)
        match nightlyBuild with
        | Build.Nightly { Number = number } ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    //Number increments for the same day build with no overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.numberNoUpperBoundaryNightlyBuild> |])>]
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
    [<Property>]
    let ``Nightly build resets the number to NumberStartValue on overflow`` (dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day) 
        let nightlyBuild = Build.Nightly { Day = dayOfMonth; Number = BuildNumber.MaxValue } |> Some                
        let nightlyBuild' = nightly nightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Build.Nightly { Number = number } ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build with different day resets the number to NumberStartValue``
        (nightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match nightlyBuild with
        | Build.Nightly { Day = dayOfMonth; Number = _ } ->
            let dayOfMonth' = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let nightlyBuild' = nightly (Some nightlyBuild) dayOfMonth'
            match nightlyBuild' with
            | Build.Nightly { Number = number' } ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
    
    // Result is always Build.BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build type always returns a BetaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let betaNightly = nightly (Some build) (byte dateTimeOffset.Day)
        test <@ betaNightly.IsBetaNightly @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build resets the number to NumberStartValue on overflow``
        (betaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day) 
        let betaNightlyBuild =
            match betaNightlyBuild with
            | Build.BetaNightly (b, _) ->
                Build.BetaNightly (b, { Day = dayOfMonth; Number = BuildNumber.MaxValue}) |> Some
            | _ -> Some betaNightlyBuild
            
        let nightlyBuild' = nightly betaNightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Build.BetaNightly (_, { Number = number }) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build with different day resets the nightly number to NumberStartValue``
        (betaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match betaNightlyBuild with
        | Build.BetaNightly (_, { Day = dayOfMonth; Number = _ }) ->
            let dayOfMonth = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let betaNightlyBuild' = nightly (Some betaNightlyBuild) dayOfMonth
            match betaNightlyBuild' with
            | Build.BetaNightly (_, { Number = number' }) ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Always keeps a beta version on nightly for BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build always keeps it's beta version when releases nightly``
        (betaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match betaNightlyBuild with
        | Build.BetaNightly ({ Number = number }, _) ->
            let betaNightlyBuild' = nightly (Some betaNightlyBuild) (byte dateTimeOffset.Day)
            match betaNightlyBuild' with
            | Build.BetaNightly ({ Number = number' }, _) ->
                test <@ number' = number @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Result is always Build.BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always returns a BetaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let betaNightly = nightly (Some build) (byte dateTimeOffset.Day)
        test <@ betaNightly.IsBetaNightly @>
        
    // Result is always Build.BetaNightly corresponding to a day and the initial number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always switch to BetaNightly with the corresponding day and initial number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let betaNightly = nightly (Some build) dayOfMonth
        match betaNightly with
        | Build.BetaNightly (_, { Day = day; Number = number }) ->
            test <@ day = dayOfMonth && number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // Beta is always keeps it's beta number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always keeps it's beta number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        match build with
        | Beta { Number = number } ->       
            let dayOfMonth = (byte dateTimeOffset.Day)
            let build' = nightly (Some build) dayOfMonth
            match build' with
            | Build.BetaNightly ({ Number = number' }, _) ->
                test <@ number' = number @>
            | _ -> test <@ false @>
        | _ -> test <@ false @>