namespace Calaf.Tests.BuildTests

open FsCheck.Xunit
open Swensen.Unquote
open Xunit

open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Domain.Build
open Calaf.Tests

module ToStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha build converts to non-empty string`` (alphaBuild: Build) =
        test <@ toString alphaBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha build string starts with the right build type prefix`` (alphaBuild: Build) =
        let alphaBuildString = toString alphaBuild
        test <@ alphaBuildString.StartsWith AlphaBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build converts to non-empty string`` (betaBuild: Build) =
        test <@ toString betaBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build string starts with the right build type prefix`` (betaBuild: Build) =
        let betaBuildString = toString betaBuild
        test <@ betaBuildString.StartsWith BetaBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcBuild> |])>]
    let ``ReleaseCandidate build converts to non-empty string`` (rcBuild: Build) =
        test <@ toString rcBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcBuild> |])>]
    let ``ReleaseCandidate build string starts with the right build type prefix`` (rcBuild: Build) =
        let rcBuildString = toString rcBuild
        test <@ rcBuildString.StartsWith ReleaseCandidateBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build converts to non-empty string`` (nightlyBuild: Build) =
        test <@ toString nightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build string starts with the right build type prefix`` (nightlyBuild: Build) =
        let nightlyBuildString = toString nightlyBuild
        test <@ nightlyBuildString.StartsWith (NightlyZeroPrefix.ToString()) @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build converts to non-empty string`` (betaNightlyBuild: Build) =
        test <@ toString betaNightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build string starts with the right build type prefix`` (betaNightlyBuild: Build) =
        let betaNightlyBuildString = toString betaNightlyBuild
        test <@ betaNightlyBuildString.StartsWith BetaBuildType @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build converts to non-empty string`` (rcNightlyBuild: Build) =
        test <@ toString rcNightlyBuild |> System.String.IsNullOrWhiteSpace |> not @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build string starts with the right build type prefix`` (rcNightlyBuild: Build) =
        let rcNightlyBuildString = toString rcNightlyBuild
        test <@ rcNightlyBuildString.StartsWith ReleaseCandidateBuildType @>
            
module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaString> |])>]
    let ``Alpha string recognizes correctly to the Alpha with number`` (alphaString: string) =
        let result = tryParseFromString alphaString
        match result with
        | Ok (Some (Build.Alpha _)) ->
            test <@ true @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaString> |])>]
    let ``Beta string recognizes correctly to the Beta with number`` (betaString: string) =
        let result = betaString |> tryParseFromString
        match result with
        | Ok (Some (Build.Beta _)) ->
            test <@ true @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcString> |])>]
    let ``ReleaseCandidate string recognizes correctly to the ReleaseCandidate with number`` (rcString: string) =
        let result = tryParseFromString rcString
        match result with
        | Ok (Some (Build.ReleaseCandidate _)) ->
            test <@ true @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyString> |])>]
    let ``Nightly string recognizes correctly to the Nightly with day and number`` (nightlyString: string) =
        let build = tryParseFromString nightlyString
        test <@ match build with | Ok (Some (Build.Nightly _)) -> true | _ -> false @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyString> |])>]
    let ``AlphaNightly string recognizes correctly to the Alpha with number`` (alphaNightlyString: string) =
        let result = tryParseFromString alphaNightlyString
        match result with
        | Ok (Some (Build.AlphaNightly _)) ->
            test <@ true @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyString> |])>]
    let ``BetaNightly string recognizes correctly to the Beta with number`` (betaNightlyString: string) =
        let result = tryParseFromString betaNightlyString
        test <@ match result with | Ok (Some (Build.BetaNightly _)) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyString> |])>]
    let ``ReleaseCandidateNightly string recognizes correctly to the ReleaseCandidate with number`` (rcNightlyString: string) =
        let result = tryParseFromString rcNightlyString
        match result with
        | Ok (Some (Build.ReleaseCandidateNightly _)) ->
            test <@ true @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Null or empty or whitespace string parses to the None`` (nullOrWhiteSpaceString: string) =
        let build = tryParseFromString nullOrWhiteSpaceString
        test <@ build = Ok None @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.wrongString> |])>]
    let ``Arbitrary string parses to BuildInvalidString error`` (wrongStringBuild: string) =
        let build = tryParseFromString wrongStringBuild
        test <@ build = Error BuildInvalidString @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.containingBetaBadString> |])>]
    let ``Bad string containing Beta parses to BuildInvalidString error`` (containingBetaBadString: string) =
        let build = tryParseFromString containingBetaBadString
        test <@ build = Error BuildInvalidString @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.containingNightlyBadString> |])>]
    let ``Bad string containing Nightly parses to BuildInvalidString error`` (containingNightlyBadString: string) =
        let result = tryParseFromString containingNightlyBadString
        result = Error BuildInvalidString
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.containingBetaNightlyBadString> |])>]
    let ``Bad string containing BetaNightly parses to BuildInvalidString error`` (containingBetaNightlyBadString: string) =
        let build = tryParseFromString containingBetaNightlyBadString
        test <@ build = Error BuildInvalidString @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.containingReleaseCandidateNightlyBadString> |])>]
    let ``Bad string containing ReleaseCandidateNightly parses to BuildInvalidString error`` (containingReleaseCandidateNightlyBadString: string) =
        let build = tryParseFromString containingReleaseCandidateNightlyBadString
        test <@ build = Error BuildInvalidString @>
        
module TryAlphaTests =
    // Result is always Build.Alpha
    [<Property>]
    let ``None build always releases an Alpha build type`` () =
        let result = tryAlpha None
        test <@ match result with | Ok (Build.Alpha _) -> true | _ -> false @>
        
    // Number is greater then assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alpha> |])>]
    let ``The number is increased for the Alpha build`` (a: AlphaBuild) =
        let a = { a with Number = Internals.preventNumberOverflow a.Number }
        let build' = tryAlpha (Some (Build.Alpha a))
        test <@ match build' with
                | Ok (Build.Alpha { Number = number' }) -> number' > a.Number
                | _ -> false @>
    
    // Number increased on the step value     
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alpha> |])>]
    let ``The number is increased on step value for the Alpha build`` (a: AlphaBuild) =
        let a = { a with Number = Internals.preventNumberOverflow a.Number }
        let build' = tryAlpha (Some (Build.Alpha a))        
        test <@ match build' with
                | Ok (Build.Alpha { Number = number' }) ->
                    number' - a.Number = NumberIncrementStep
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly always releases aa Alpha build type`` (build: Build) =
        let build' = tryAlpha (Some build)
        test <@ match build' with
                | Ok (Build.Alpha _) -> true
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``Nightly always releases a Alpha build with NumberStartValue`` (n: NightlyBuild) =
        let build' = tryAlpha (Some (Build.Nightly n))   
        test <@ match build' with
                | Ok (Build.Alpha { Number = number' }) -> number' = NumberStartValue
                | _ -> false @>    
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha always releases an Alpha build type`` (build: Build) =
        let build' = tryAlpha (Some build)
        test <@ match build' with | Ok (Build.Alpha _) -> true | _ -> false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alpha> |])>]
    let ``Alpha number resets the number to NumberStartValue on overflow when releasing to Alpha`` (a: AlphaBuild) =
        let a = { a with Number = BuildNumber.MaxValue }
        let build' = tryAlpha (Some (Build.Alpha a))        
        test <@ match build' with
                | Ok (Build.Alpha { Number = number' }) ->
                    number' = NumberStartValue
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always resets it's Alpha's overflowed build number when releasing Alpha`` (build: Build) =
        let build =
            match build with
            | AlphaNightly (_, { Day = day; Number = nightlyNumber }) ->
                AlphaNightly({ Number = BuildNumber.MaxValue }, { Day = day; Number = nightlyNumber })
            | _ -> build
        let build' = tryAlpha (Some build)
        match build' with
        | Ok (Build.Alpha { Number = alphaNumber' }) ->
            test <@ alphaNumber' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always releases an Alpha build type`` (build: Build) =
        let build' = tryAlpha (Some build)
        test <@ match build' with | Ok (Build.Alpha _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always switches to Alpha and increases the build number when releasing Alpha`` (build: Build) =        
        let build' = tryAlpha (Some build)
        match (build, build') with
        | Build.AlphaNightly ({ Number = alphaNumber }, _), Ok (Build.Alpha { Number = alphaNumber' }) ->
            test <@ alphaNumber' <> alphaNumber @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta always returns BuildDowngradeProhibited error when releasing Alpha`` (build: Build) =
        let build' = tryAlpha (Some build)
        test <@ match build' with | Error BuildDowngradeProhibited -> true | _ -> false @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly always returns BuildDowngradeProhibited error when releasing Alpha`` (build: Build) =
        let build' = tryAlpha (Some build)
        test <@ match build' with | Error BuildDowngradeProhibited -> true | _ -> false @>

module TryBetaTests =
    // Result is always Build.Beta
    [<Property>]
    let ``None build always releases a Beta build type`` () =
        let result = tryBeta None
        test <@ match result with | Ok (Build.Beta _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta always releases a Beta build type`` (build: Build) =
        let build' = tryBeta (Some build)
        test <@ match build' with | Ok (Build.Beta _) -> true | _ -> false @>
        
    // Number is greater then assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The number is increased for Beta build`` (b: BetaBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = tryBeta (Some (Build.Beta b))
        test <@ match build' with
                | Ok (Build.Beta { Number = number' }) -> number' > b.Number
                | _ -> false @>
    
    // Number increased on the step value     
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The number is increased on step value for Beta build`` (b: BetaBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = tryBeta (Some (Build.Beta b))        
        test <@ match build' with
                | Ok (Build.Beta { Number = number' }) ->
                    number' - b.Number = NumberIncrementStep
                | _ -> false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta> |])>]
    let ``The Beta number resets the number to NumberStartValue on overflow`` (b: BetaBuild) =
        let b = { b with Number = BuildNumber.MaxValue }
        let build' = tryBeta (Some (Build.Beta b))        
        test <@ match build' with
                | Ok (Build.Beta { Number = number' }) ->
                    number' = NumberStartValue
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly always releases a Beta build type`` (build: Build) =
        let build' = tryBeta (Some build)
        test <@ match build' with
                | Ok (Build.Beta _) -> true
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``Nightly always releases a Beta build with NumberStartValue`` (n: NightlyBuild) =
        let build' = tryBeta (Some (Build.Nightly n))   
        test <@ match build' with
                | Ok (Build.Beta { Number = number' }) -> number' = NumberStartValue
                | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha always releases a Beta build type when releasing Beta`` (build: Build) =
        let build' = tryBeta (Some build)
        test <@ match build' with | Ok (Build.Beta _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha always switches to Beta and starts with the initial number of Beta build type when releasing Beta`` (build: Build) =        
        let build' = tryBeta (Some build)
        match build' with
        | Ok (Build.Beta { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always releases a Beta build type when releasing Beta`` (build: Build) =
        let build' = tryBeta (Some build)
        test <@ match build' with | Ok (Build.Beta _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always switches to Beta and starts with the initial number of Beta build type when releasing Beta`` (build: Build) =        
        let build' = tryBeta (Some build)
        match build' with
        | Ok (Build.Beta { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly always releases a Beta build type`` (build: Build) =
        let build' = tryBeta (Some build)
        test <@ match build' with | Ok (Build.Beta _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly always increases number of Beta build type when releases beta`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = tryBeta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Ok (Build.Beta { Number = number' }) ->
            test <@ number' > b.Number @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly always increases number by step when releases beta`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = Internals.preventNumberOverflow b.Number }
        let build' = tryBeta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Ok (Build.Beta { Number = number' }) ->
            test <@ number' - b.Number = NumberIncrementStep @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.beta>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``BetaNightly resets the number to NumberStartValue on overflow`` (b: BetaBuild, n: NightlyBuild) =
        let b = { b with Number = BuildNumber.MaxValue }
        let build' = tryBeta (Some (Build.BetaNightly (b, n)))   
        match build' with
        | Ok (Build.Beta { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
module TryReleaseCandidateTests =
    // Result is always Build.ReleaseCandidate
    [<Fact>]
    let ``None build always releases a ReleaseCandidate build type`` () =
        let result = tryReleaseCandidate None
        test <@ match result with | Ok (Build.ReleaseCandidate _) -> true | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcBuild> |])>]
    let ``ReleaseCandidate always releases a ReleaseCandidate build type`` (build: Build) =
        let build' = tryReleaseCandidate (Some build)
        test <@ match build' with | Ok (Build.ReleaseCandidate _) -> true | _ -> false @>
        
    // Number is greater then assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rc> |])>]
    let ``The number is increased for ReleaseCandidate build (no number overflow)`` (rc: ReleaseCandidateBuild) =
        let rc = { rc with Number = Internals.preventNumberOverflow rc.Number }
        let build' = tryReleaseCandidate (Some (Build.ReleaseCandidate rc))
        test <@ match build' with
                | Ok (Build.ReleaseCandidate { Number = number' }) -> number' > rc.Number
                | _ -> false @>
    
    // Number increased on the step value     
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rc> |])>]
    let ``The number is increased on step value for ReleaseCandidate build (no number overflow)`` (rc: ReleaseCandidateBuild) =
        let rc = { rc with Number = Internals.preventNumberOverflow rc.Number }
        let build' = tryReleaseCandidate (Some (Build.ReleaseCandidate rc))        
        test <@ match build' with
                | Ok (Build.ReleaseCandidate { Number = number' }) ->
                    number' - rc.Number = NumberIncrementStep
                | _ -> false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rc> |])>]
    let ``The ReleaseCandidate number resets the number to NumberStartValue on overflow (max number)`` (rc: ReleaseCandidateBuild) =
        let rc = { rc with Number = BuildNumber.MaxValue }
        let build' = tryReleaseCandidate (Some (Build.ReleaseCandidate rc))        
        test <@ match build' with
                | Ok (Build.ReleaseCandidate { Number = number' }) ->
                    number' = NumberStartValue
                | _ -> false @>        
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly always releases a ReleaseCandidate build type and starts with the initial number`` (build: Build) =
        let build' = tryReleaseCandidate (Some build)   
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha always releases a ReleaseCandidate build type and starts with the initial number`` (build: Build) =
        let build' = tryReleaseCandidate (Some build)   
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly always releases a ReleaseCandidate build type and starts with the initial number`` (build: Build) =        
        let build' = tryReleaseCandidate (Some build)
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly always releases a ReleaseCandidate build type and starts with the initial number`` (build: Build) =        
        let build' = tryReleaseCandidate (Some build)   
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rc>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``ReleaseCandidateNightly always releases a ReleaseCandidate build type and increases number of the ReleaseCandidate Number (no number overflow)`` (rc: ReleaseCandidateBuild, n: NightlyBuild) =
        let rc = { rc with Number = Internals.preventNumberOverflow rc.Number }
        let build' = tryReleaseCandidate (Some (Build.ReleaseCandidateNightly (rc, n)))   
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' > rc.Number @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rc>; typeof<Arbitrary.Build.Nightly.nightly> |])>]
    let ``ReleaseCandidateNightly resets the number to NumberStartValue on overflow`` (rc: ReleaseCandidateBuild, n: NightlyBuild) =
        let rc = { rc with Number = BuildNumber.MaxValue }
        let build' = tryReleaseCandidate (Some (Build.ReleaseCandidateNightly (rc, n)))   
        match build' with
        | Ok (Build.ReleaseCandidate { Number = number' }) ->
            test <@ number' = NumberStartValue @>
        | _ -> test <@ false @>
        
module TryNightlyTests =
    // Result is always Build.Nightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuildOption> |])>]
    let ``Nightly build type always returns a Nightly build type when releases nightly``
        (build: Build option, dateTimeOffset: System.DateTimeOffset) =
        let nightly = tryNightly build (byte dateTimeOffset.Day)
        test <@ match nightly with
                | Ok (Build.Nightly _) -> true
                | _ -> false @>
        
    // Day is correctly assigned
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuildOption> |])>]
    let ``The day of the month is correctly assigned to the Nightly build``
        (nightlyBuild: Build option, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let nightlyBuild' = tryNightly nightlyBuild dayOfMonth
        match nightlyBuild' with
        | Ok (Build.Nightly { Day = actualDay }) ->
            test <@ actualDay = dayOfMonth @>
        | _ -> test <@ false @>
        
    // Number starts at NumberStartValue if no current build
    [<Property>]
    let ``Nightly build starts at NumberStartValue if no current build``
        (dateTimeOffset: System.DateTimeOffset) =
        let nightlyBuild = tryNightly None (byte dateTimeOffset.Day)
        match nightlyBuild with
        | Ok (Build.Nightly { Number = number }) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    //Number increments for the same day build with no overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.numberNoUpperBoundaryNightlyBuild> |])>]
    let ``Nightly build with no upper boundary number increments the number for the same day nightly build`` (nightlyBuild: Build) =
        match nightlyBuild with
        | Build.Nightly { Day = dayOfMonth; Number = number } ->
            let nightlyBuild' = tryNightly (Some nightlyBuild) dayOfMonth            
            match nightlyBuild' with
            | Ok (Build.Nightly { Number = number' }) ->
                test <@ number' - number = NumberIncrementStep @>
            | _ -> test <@ false @>
        | _ -> test <@ false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property>]
    let ``Nightly build resets the number to NumberStartValue on overflow`` (dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day) 
        let nightlyBuild = Build.Nightly { Day = dayOfMonth; Number = BuildNumber.MaxValue } |> Some                
        let nightlyBuild' = tryNightly nightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Ok (Build.Nightly { Number = number }) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Nightly.nightlyBuild> |])>]
    let ``Nightly build with different day resets the number to NumberStartValue``
        (nightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match nightlyBuild with
        | Build.Nightly { Day = dayOfMonth; Number = _ } ->
            let dayOfMonth' = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let nightlyBuild' = tryNightly (Some nightlyBuild) dayOfMonth'
            match nightlyBuild' with
            | Ok (Build.Nightly { Number = number' }) ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Result is always Build.AlphaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly build type always returns a AlphaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let alphaNightly = tryNightly (Some build) (byte dateTimeOffset.Day)
        test <@ match alphaNightly with
                | Ok (Build.AlphaNightly _) -> true
                | _ -> false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly build resets the number to NumberStartValue on overflow``
        (alphaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day) 
        let alphaNightlyBuild =
            match alphaNightlyBuild with
            | Build.AlphaNightly (a, _) ->
                Build.AlphaNightly (a, { Day = dayOfMonth; Number = BuildNumber.MaxValue}) |> Some
            | _ -> Some alphaNightlyBuild
            
        let nightlyBuild' = tryNightly alphaNightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Ok (Build.AlphaNightly (_, { Number = number })) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly build with different day resets the nightly number to NumberStartValue``
        (alphaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match alphaNightlyBuild with
        | Build.AlphaNightly (_, { Day = dayOfMonth; Number = _ }) ->
            let dayOfMonth = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let alphaNightlyBuild' = tryNightly (Some alphaNightlyBuild) dayOfMonth
            match alphaNightlyBuild' with
            | Ok (Build.AlphaNightly (_, { Number = number' })) ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Always keeps a beta version on nightly for AlphaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.AlphaNightly.alphaNightlyBuild> |])>]
    let ``AlphaNightly build always keeps it's alpha version when releases nightly``
        (alphaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match alphaNightlyBuild with
        | Build.AlphaNightly ({ Number = number }, _) ->
            let alphaNightlyBuild' = tryNightly (Some alphaNightlyBuild) (byte dateTimeOffset.Day)
            match alphaNightlyBuild' with
            | Ok (Build.AlphaNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
    
    // Result is always Build.BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build type always returns a BetaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let betaNightly = tryNightly (Some build) (byte dateTimeOffset.Day)
        test <@ match betaNightly with
                | Ok (Build.BetaNightly _) -> true
                | _ -> false @>
        
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
            
        let nightlyBuild' = tryNightly betaNightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Ok (Build.BetaNightly (_, { Number = number })) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build with different day resets the nightly number to NumberStartValue``
        (betaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match betaNightlyBuild with
        | Build.BetaNightly (_, { Day = dayOfMonth; Number = _ }) ->
            let dayOfMonth = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let betaNightlyBuild' = tryNightly (Some betaNightlyBuild) dayOfMonth
            match betaNightlyBuild' with
            | Ok (Build.BetaNightly (_, { Number = number' })) ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Always keeps a beta version on nightly for BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.BetaNightly.betaNightlyBuild> |])>]
    let ``BetaNightly build always keeps it's beta version when releases nightly``
        (betaNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match betaNightlyBuild with
        | Build.BetaNightly ({ Number = number }, _) ->
            let betaNightlyBuild' = tryNightly (Some betaNightlyBuild) (byte dateTimeOffset.Day)
            match betaNightlyBuild' with
            | Ok (Build.BetaNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Result is always Build.ReleaseCandidateNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build type always returns a ReleaseCandidateNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let rcNightly = tryNightly (Some build) (byte dateTimeOffset.Day)
        match rcNightly with
        | Ok (Build.ReleaseCandidateNightly _) -> test <@ true @>
        | _ -> test <@ false @>
        
    // Number resets to NumberStartValue on overflow
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build resets the number to NumberStartValue on overflow``
        (rcNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day) 
        let rcNightlyBuild =
            match rcNightlyBuild with
            | Build.ReleaseCandidateNightly (rc, _) ->
                Build.ReleaseCandidateNightly (rc, { Day = dayOfMonth; Number = BuildNumber.MaxValue}) |> Some
            | _ -> Some rcNightlyBuild
            
        let nightlyBuild' = tryNightly rcNightlyBuild dayOfMonth        
        match nightlyBuild' with
        | Ok (Build.ReleaseCandidateNightly (_, { Number = number })) ->
            test <@ number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // New day resets the number to the NumberStartValue
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build with different day resets the nightly number to NumberStartValue``
        (rcNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match rcNightlyBuild with
        | Build.ReleaseCandidateNightly (_, { Day = dayOfMonth; Number = _ }) ->
            let dayOfMonth = Internals.uniqueDay (dayOfMonth, dateTimeOffset)
            let rcNightlyBuild' = tryNightly (Some rcNightlyBuild) dayOfMonth
            match rcNightlyBuild' with
            | Ok (Build.ReleaseCandidateNightly (_, { Number = number' })) ->
                test <@ number' = NumberStartValue @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Always keeps a release candidate version on nightly for ReleaseCandidateNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidateNightly.rcNightlyBuild> |])>]
    let ``ReleaseCandidateNightly build always keeps it's release candidate's version when releases nightly``
        (rcNightlyBuild: Build, dateTimeOffset: System.DateTimeOffset) =
        match rcNightlyBuild with
        | Build.ReleaseCandidateNightly ({ Number = number }, _) ->
            let rcNightlyBuild' = tryNightly (Some rcNightlyBuild) (byte dateTimeOffset.Day)
            match rcNightlyBuild' with
            | Ok (Build.ReleaseCandidateNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>            
        | _ -> test <@ false @>
        
    // Result is always Build.AlphaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha build type always returns a AlphaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let alphaNightly = tryNightly (Some build) (byte dateTimeOffset.Day)
        test <@ match alphaNightly with
                | Ok (Build.AlphaNightly _) -> true
                | _ -> false @>
        
    // Result is always Build.AlphaNightly corresponding to a day and the initial number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha build type always switch to AlphaNightly with the corresponding day and initial number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let alphaNightly = tryNightly (Some build) dayOfMonth
        match alphaNightly with
        | Ok (Build.AlphaNightly (_, { Day = day; Number = number })) ->
            test <@ day = dayOfMonth && number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // Alpha is always keeps it's beta number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Alpha.alphaBuild> |])>]
    let ``Alpha build type always keeps it's alpha number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        match build with
        | Alpha { Number = number } ->       
            let dayOfMonth = (byte dateTimeOffset.Day)
            let build' = tryNightly (Some build) dayOfMonth
            match build' with
            | Ok (Build.AlphaNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>
        | _ -> test <@ false @>
        
    // Result is always Build.BetaNightly
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always returns a BetaNightly build type when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let betaNightly = tryNightly (Some build) (byte dateTimeOffset.Day)
        test <@ match betaNightly with
                | Ok (Build.BetaNightly _) -> true
                | _ -> false @>
        
    // Result is always Build.BetaNightly corresponding to a day and the initial number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always switch to BetaNightly with the corresponding day and initial number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let betaNightly = tryNightly (Some build) dayOfMonth
        match betaNightly with
        | Ok (Build.BetaNightly (_, { Day = day; Number = number })) ->
            test <@ day = dayOfMonth && number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // Beta is always keeps it's beta number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.Beta.betaBuild> |])>]
    let ``Beta build type always keeps it's beta number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        match build with
        | Beta { Number = number } ->       
            let dayOfMonth = (byte dateTimeOffset.Day)
            let build' = tryNightly (Some build) dayOfMonth
            match build' with
            | Ok (Build.BetaNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>
        | _ -> test <@ false @>    
        
    // Result is always Build.ReleaseCandidateNightly corresponding to a day and the initial number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcBuild> |])>]
    let ``ReleaseCandidate always switch to ReleaseCandidateNightly with the corresponding day and initial number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = (byte dateTimeOffset.Day)
        let rcNightly = tryNightly (Some build) dayOfMonth
        match rcNightly with
        | Ok (Build.ReleaseCandidateNightly (_, { Day = day; Number = number })) ->
            test <@ day = dayOfMonth && number = NumberStartValue @>
        | _ -> test <@ false @>
        
    // ReleaseCandidate is always keeps it's ReleaseCandidate number
    [<Property(Arbitrary = [| typeof<Arbitrary.Build.ReleaseCandidate.rcBuild> |])>]
    let ``ReleaseCandidate build type always keeps it's release candidate's number when releases nightly``
        (build: Build, dateTimeOffset: System.DateTimeOffset) =
        match build with
        | ReleaseCandidate { Number = number } ->       
            let dayOfMonth = (byte dateTimeOffset.Day)
            let build' = tryNightly (Some build) dayOfMonth
            match build' with
            | Ok (Build.ReleaseCandidateNightly ({ Number = number' }, _)) ->
                test <@ number' = number @>
            | _ -> test <@ false @>
        | _ -> test <@ false @>