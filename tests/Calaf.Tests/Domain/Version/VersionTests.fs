namespace Calaf.Tests.VersionTests

open FsCheck.Xunit
open Xunit
open Swensen.Unquote

open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Version
open Calaf.Tests

module TryParseFromStringTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.String> |])>]
    let ``Calendar version string always parses to CalendarVersion and then back to the same Calendar version string`` (releaseString: string) =        
        let version = tryParseFromString releaseString
        let releaseString' =
            version
            |> Option.map (function | CalVer v -> toString v | _ -> System.String.Empty)
            |> Option.defaultValue System.String.Empty 
        test <@ System.String.Equals(releaseString', releaseString, System.StringComparison.InvariantCultureIgnoreCase) @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.String> |])>]
    let ``Calendar version string always parses to CalendarVersion`` (releaseString: string) =        
        let version = tryParseFromString releaseString        
        test <@ version.Value.IsCalVer @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.String> |])>]
    let ``Stable calendar version string always parses to the CalendarVersion without Build`` (stableString: string) =        
        let version = tryParseFromString stableString
        let hasNoBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.IsNone | _ -> false)
            |> Option.defaultValue false
        test <@ hasNoBuild @> 
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Alpha.String> |])>]
    let ``Alpha calendar version string always parses to CalendarVersion with Alpha Build containing its original values`` (alphaString: string) =        
        let version = tryParseFromString alphaString        
        match version with
        | Some (CalVer { Year = year; Month = month; Patch = Some patch; Build = Some (Build.Alpha({ Number = number })) }) ->
            test <@
                alphaString.Contains $"{year}" &&
                alphaString.Contains $"{month}" &&
                alphaString.Contains $"{patch}" &&
                alphaString.Contains $"{number}" @>
        | Some (CalVer { Year = year; Month = month; Patch = None; Build = Some (Build.Alpha({ Number = number })) }) ->
            test <@
                alphaString.Contains $"{year}" &&
                alphaString.Contains $"{month}" &&
                alphaString.Contains $"{number}" @>
        | _ -> test <@ false @> 
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.String> |])>]
    let ``Beta calendar version string always parses to CalendarVersion with Beta Build containing its original values`` (betaString: string) =        
        let version = tryParseFromString betaString
        match version with
        | Some (CalVer { Year = year; Month = month; Patch = Some patch; Build = Some (Build.Beta({ Number = number })) }) ->
            test <@
                betaString.Contains $"{year}" &&
                betaString.Contains $"{month}" &&
                betaString.Contains $"{patch}" &&
                betaString.Contains $"{number}" @>
        | Some (CalVer { Year = year; Month = month; Patch = None; Build = Some (Build.Beta({ Number = number })) }) ->
            test <@
                betaString.Contains $"{year}" &&
                betaString.Contains $"{month}" &&
                betaString.Contains $"{number}" @>
        | _ -> test <@ false @> 
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.String> |])>]
    let ``Nightly calendar version string always parses to the CalendarVersion with Nightly Build`` (nightlyString: string) =        
        let version = tryParseFromString nightlyString
        match version with
        | Some (CalVer { Year = year; Month = month; Patch = Some patch; Build = Some (Build.Nightly({ Day = day; Number = number })) }) ->
            test <@
                nightlyString.Contains $"{year}" &&
                nightlyString.Contains $"{month}" &&
                nightlyString.Contains $"{patch}" &&
                nightlyString.Contains $"{day}" &&
                nightlyString.Contains $"{number}" @>
        | Some (CalVer { Year = year; Month = month; Patch = None; Build = Some (Build.Nightly({ Day = day; Number = number })) }) ->
            test <@
                nightlyString.Contains $"{year}" &&
                nightlyString.Contains $"{month}" &&
                nightlyString.Contains $"{day}" &&
                nightlyString.Contains $"{number}" @>
        | _ -> test <@ false @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AlphaNightly.String> |])>]
    let ``Alpha-Nightly calendar version string always parses to the CalendarVersion with AlphaNightly Build`` (alphaNightlyString: string) =        
        let version = tryParseFromString alphaNightlyString
        match version with
        | Some (CalVer { Year = year; Month = month; Patch = Some patch; Build = Some (Build.AlphaNightly({ Number = alphaNumber }, { Day = day; Number = nightlyNumber })) }) ->
            test <@
                alphaNightlyString.Contains $"{year}" &&
                alphaNightlyString.Contains $"{month}" &&
                alphaNightlyString.Contains $"{patch}" &&
                alphaNightlyString.Contains $"{alphaNumber}" &&
                alphaNightlyString.Contains $"{day}" &&
                alphaNightlyString.Contains $"{nightlyNumber}" @>
        | Some (CalVer { Year = year; Month = month; Patch = None; Build = Some (Build.AlphaNightly({ Number = alphaNumber }, { Day = day; Number = number })) }) ->
            test <@
                alphaNightlyString.Contains $"{year}" &&
                alphaNightlyString.Contains $"{month}" &&
                alphaNightlyString.Contains $"{alphaNumber}" &&
                alphaNightlyString.Contains $"{day}" &&
                alphaNightlyString.Contains $"{number}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.String> |])>]
    let ``Beta-Nightly calendar version string always parses to the CalendarVersion with BetaNightly Build`` (betaNightlyString: string) =        
        let version = tryParseFromString betaNightlyString
        match version with
        | Some (CalVer { Year = year; Month = month; Patch = Some patch; Build = Some (Build.BetaNightly({ Number = betaNumber }, { Day = day; Number = nightlyNumber })) }) ->
            test <@
                betaNightlyString.Contains $"{year}" &&
                betaNightlyString.Contains $"{month}" &&
                betaNightlyString.Contains $"{patch}" &&
                betaNightlyString.Contains $"{betaNumber}" &&
                betaNightlyString.Contains $"{day}" &&
                betaNightlyString.Contains $"{nightlyNumber}" @>
        | Some (CalVer { Year = year; Month = month; Patch = None; Build = Some (Build.BetaNightly({ Number = betaNumber }, { Day = day; Number = number })) }) ->
            test <@
                betaNightlyString.Contains $"{year}" &&
                betaNightlyString.Contains $"{month}" &&
                betaNightlyString.Contains $"{betaNumber}" &&
                betaNightlyString.Contains $"{day}" &&
                betaNightlyString.Contains $"{number}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.ShortString> |])>]
    let ``Short Calendar Version string always parses to Calendar Version without Patch`` (releaseString: string) =
        let version = tryParseFromString releaseString
        let hasNoPatch =
            version
            |> Option.map (function | CalVer v -> v.Patch.IsNone | _ -> false)
            |> Option.defaultValue false
        test <@ hasNoPatch @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.PatchString> |])>]
    let ``Patch Calendar Version string always parses to Calendar Version with Patch`` (releaseString: string) =
        let version = tryParseFromString releaseString
        let hasPatch =
            version
            |> Option.map (function | CalVer v -> v.Patch.IsSome | _ -> false)
            |> Option.defaultValue false
        test <@ hasPatch @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.SematicVersion.semanticVersionStr> |])>]
    let ``Valid SemVer string always parses to SemanticVersion`` (releaseString: string) =
        let version = tryParseFromString releaseString
        let isSemVer =
            version
            |> Option.map (function | SemVer _ -> true | _ -> false)
            |> Option.defaultValue false
        test <@ isSemVer @>      
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.WhiteSpaceLeadingTrailingString> |])>]
    let ``Leading/trailing whitespace in input is ignored`` (whiteSpaceLeadingTrailingCalendarVersionString: string) =        
        match tryParseFromString whiteSpaceLeadingTrailingCalendarVersionString with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.invalidThreePartString> |])>]
    let ``Invalid CalVer/SemVer but valid three-part format parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Null or empty or whitespace string parses to None`` (badVersion: string) =
        badVersion |> tryParseFromString = None        
        
module TryParseFromTagTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.TagStrictString> |])>]
    let ``Calendar version tag strict string always parses to CalendarVersion and then back to the same Calendar version tag string``
        (tag: string) =        
        let version = tryParseFromTag tag
        let tag' =
            version
            |> Option.map (function | CalVer v -> toTagName v | _ -> System.String.Empty)
            |> Option.defaultValue System.String.Empty 
        test <@ System.String.Equals(tag', tag, System.StringComparison.InvariantCultureIgnoreCase) @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.TagString> |])>]
    let ``Calendar Version tag string always parses to Calendar Version`` (tag: string) =        
        let version = tryParseFromTag tag
        test <@ version.Value.IsCalVer @>        
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.TagString> |])>]
    let ``Stable Calendar Version tag string always parses to Calendar Version without Build`` (tag: string) =         
        let version = tryParseFromTag tag
        let hasNoBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.IsNone | _ -> false)
            |> Option.defaultValue false
        test <@ hasNoBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Alpha.TagString> |])>]
    let ``Alpha Calendar Version tag string always parses to Calendar Version with Alpha Build`` (tag: string) =         
        let version = tryParseFromTag tag
        match version with
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = Some patch
              Build = Some(Alpha({ Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{patch}" &&
                    tag.Contains $"{number}" @>
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = None
              Build = Some(Alpha({ Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{number}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.TagString> |])>]
    let ``Beta Calendar Version tag string always parses to Calendar Version with Beta Build`` (tag: string) =         
        let version = tryParseFromTag tag
        match version with
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = Some patch
              Build = Some(Beta({ Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{patch}" &&
                    tag.Contains $"{number}" @>
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = None
              Build = Some(Beta({ Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{number}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.TagString> |])>]
    let ``Nightly Calendar Version tag string always parses to Calendar Version with Nightly Build`` (tag: string) =         
        let version = tryParseFromTag tag
        match version with
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = Some patch
              Build = Some(Nightly({ Day = day; Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{patch}" &&
                    tag.Contains $"{day}" &&
                    tag.Contains $"{number}" @>
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = None
              Build = Some(Nightly({ Day = day; Number = number })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{day}" &&
                    tag.Contains $"{number}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AlphaNightly.TagString> |])>]
    let ``Alpha-Nightly Calendar Version tag string always parses to Calendar Version with AlphaNightly Build`` (tag: string) =         
        let version = tryParseFromTag tag
        match version with
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = Some patch
              Build = Some(AlphaNightly({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{patch}" &&
                    tag.Contains $"{alphaNumber}" &&
                    tag.Contains $"{nightlyDay}" &&
                    tag.Contains $"{nightlyNumber}" @>
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = None
              Build = Some(AlphaNightly({ Number = alphaNumber },{ Day = nightlyDay; Number = nightlyNumber })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{alphaNumber}" &&
                    tag.Contains $"{nightlyDay}" &&
                    tag.Contains $"{nightlyNumber}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.TagString> |])>]
    let ``Beta-Nightly Calendar Version tag string always parses to Calendar Version with BetaNightly Build`` (tag: string) =         
        let version = tryParseFromTag tag
        match version with
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = Some patch
              Build = Some(BetaNightly({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{patch}" &&
                    tag.Contains $"{betaNumber}" &&
                    tag.Contains $"{nightlyDay}" &&
                    tag.Contains $"{nightlyNumber}" @>
        | Some (CalVer(
            { Year = year
              Month = month
              Patch = None
              Build = Some(BetaNightly({ Number = betaNumber },{ Day = nightlyDay; Number = nightlyNumber })) })) ->
            test <@ tag.Contains $"{year}" &&
                    tag.Contains $"{month}" &&
                    tag.Contains $"{betaNumber}" &&
                    tag.Contains $"{nightlyDay}" &&
                    tag.Contains $"{nightlyNumber}" @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.SematicVersion.semanticVersionStr> |])>]
    let ``Correct Sematic Version string parses to Semantic Version`` (tag: string) =
        let version = tryParseFromTag tag
        let hasBetaNightlyBuild =
            version
            |> Option.map (function | SemVer _ -> true | _ -> false)
            |> Option.defaultValue false
        test <@ hasBetaNightlyBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.WhiteSpaceLeadingTrailingTagString> |])>]
    let ``Leading/trailing whitespaces in Calendar Version tag string is ignored`` (tag: string) =        
        let version = tryParseFromTag tag
        test <@ version.Value.IsCalVer @>        
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.ShortTagString> |])>]
    let ``Short Calendar Version tag string always parses to Calendar Version without Patch`` (tag: string) =
        let version = tryParseFromTag tag
        let hasNoPatch =
            version
            |> Option.map (function | CalVer v -> v.Patch.IsNone | _ -> false)
            |> Option.defaultValue false
        test <@ hasNoPatch @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.PatchTagString> |])>]
    let ``Patch Calendar Version tag string always parses to Calendar Version with Patch`` (tag: string) =
        let version = tryParseFromTag tag
        let hasPatch =
            version
            |> Option.map (function | CalVer v -> v.Patch.IsSome | _ -> false)
            |> Option.defaultValue false
        test <@ hasPatch @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |])>]
    let ``Invalid string parses to Unsupported`` (tag: string) =
        let version = tryParseFromTag tag
        test <@ version.Value.IsUnsupported @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.invalidThreePartString> |])>]
    let ``Invalid CalVer/SemVer but valid three-part format parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromTag = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Null or empty or whitespace string parses to None`` (badVersion: string) =
        badVersion |> tryParseFromTag = None
        
module TryMaxTests =
    [<Fact>]
    let ``Same date beta, nightly, beta-nightly versions return beta-nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 2u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 2u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 2u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
        ]
        let expected = versions[4] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Same date stable, beta, nightly, beta-nightly + higher beta versions return stable`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 2us }) }
        ]
        let expected = versions[1] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Same date stable, beta, nightly, beta-nightly + higher patch stable versions return higher patch stable version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
            { Year = 9999us; Month = 12uy; Patch = Some 2u; Build = None }
        ]
        let expected = versions[5] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Same date stable, beta, nightly, beta-nightly + higher month stable versions return higher month stable version`` () =
        let versions = [
            { Year = 9999us; Month = 7uy; Patch = None; Build = None }
            { Year = 9999us; Month = 7uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 7uy; Patch = Some 1u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 7uy; Patch = Some 1u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 7uy; Patch = Some 1u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
            { Year = 9999us; Month = 8uy; Patch = None; Build = None }
        ]
        let expected = versions[5] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Beta short versions return higher beta version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Beta { Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Beta { Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Beta { Number = System.UInt16.MaxValue }) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Beta patch versions return higher beta version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = System.UInt16.MaxValue }) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Beta patch versions + higher month return beta version with higher month`` () =
        let versions = [
            { Year = 9999us; Month = 11uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 11uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 10us }) }
            { Year = 9999us; Month = 11uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = System.UInt16.MaxValue }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Beta { Number = 1us }) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
    
    [<Fact>]
    let ``Nightly short versions return higher nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 31uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 31uy; Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 31uy; Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 31uy; Number = System.UInt16.MaxValue }) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Nightly patch versions return higher nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 31uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 31uy; Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 31uy; Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 31uy; Number = System.UInt16.MaxValue }) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Nightly short versions with different days return higher nightly version with the highest day`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 30uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 30uy; Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 30uy; Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 30uy; Number = System.UInt16.MaxValue }) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (Nightly { Day = 31uy; Number = 1us }) }
        ]
        let expected = versions[4] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Nightly patch versions with different days return higher nightly version with the highest day`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 30uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 30uy; Number = 5us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 30uy; Number = 10us }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 30uy; Number = System.UInt16.MaxValue }) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (Nightly { Day = 31uy; Number = 1us }) }
        ]
        let expected = versions[4] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``BetaNightly short versions return higher beta-nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 1us })) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 5us })) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 10us })) }
            { Year = 9999us; Month = 12uy; Patch = None; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = System.UInt16.MaxValue })) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``BetaNightly patch versions return higher beta-nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 1us })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 5us })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 10us })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = System.UInt16.MaxValue })) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``BetaNightly patch versions return higher beta-nightly version with highest day and numbers`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue - 1us }, { Day = 31uy; Number = System.UInt16.MaxValue })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue - 1us }, { Day = 31uy; Number = System.UInt16.MaxValue })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 30uy; Number = System.UInt16.MaxValue })) }
            { Year = 9999us; Month = 12uy; Patch = Some Patch.MaxValue; Build = Some (BetaNightly ({ Number = System.UInt16.MaxValue }, { Day = 31uy; Number = 1us })) }
        ]
        let expected = versions[3] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
    
    [<Fact>]
    let ``Empty array returns None`` () =
        let versions = [||]
        let max = versions |> tryMax
        test <@ max = None  @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |] )>]
    let ``Single element returns this element`` (calendarVersion: CalendarVersion) =
        let versions = [| calendarVersion |]
        let max = versions |> tryMax
        test <@ max = Some calendarVersion @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalsArray> |])>]
    let ``Multiple elements returns the element of this sequence`` (calendarVersions: CalendarVersion[]) =
        let contains =
            calendarVersions
            |> tryMax
            |> Option.map (fun v -> calendarVersions |> Seq.contains v)
            |> Option.defaultValue false
        test <@ contains @>      
   
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalsArray> |])>]
    let ``Order invariance (array reversal does not affect result)`` (calendarVersions: CalendarVersion[]) =
        test <@ tryMax calendarVersions = tryMax (Array.randomShuffle calendarVersions) @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalsArray> |])>]
    let ``Duplicate tolerance`` (calendarVersions: CalendarVersion[]) =
        let max = tryMax calendarVersions
        let duplicated = Array.append calendarVersions [| max.Value |]
        let max' = tryMax duplicated
        test <@ max' = max @>

module TryAlphaTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Alpha.Accidental> |])>]
    let ``Alpha CalendarVersion releases to the new alpha``
        (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let release = tryAlpha v dateTimeOffset
        
        let expectedYear = dateTimeOffset.Year
        let expectedMonth = dateTimeOffset.Month
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = _; Build = b } ->
                int year = expectedYear &&
                int month = expectedMonth &&
                v.Build <> b
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``Beta CalendarVersion prohibits to alpha``
        (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let release = tryAlpha v dateTimeOffset
        test <@
            match release with
            | Error Calaf.Domain.DomainError.BuildDowngradeProhibited -> true
            | _ -> false @>        
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Patch> |])>]
    let ``Stable with Patch CalendarVersion releases alpha, set appropriate Year, Month, no Patch, adds Build type Alpha``
        (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (v, dateTimeOffset)
        let v' = tryAlpha v dateTimeOffset
        
        let expectedYear = dateTimeOffset.Year
        let expectedMonth = dateTimeOffset.Month
        test <@
            match v' with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Alpha a) } ->
                int year = expectedYear &&
                int month = expectedMonth &&
                patch.IsNone &&
                a.Number = Calaf.Domain.Build.NumberStartValue
            | _ -> false @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AlphaNightly.Accidental> |])>]
    let ``AlphaNightly CalendarVersion releases alpha, changes an alpha number but keeps the same Year, Month, Patch when the year and month are same``
        (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =        
        let v' = tryAlpha v dateTimeOffset
        
        let expectedYear = dateTimeOffset.Year
        let expectedMonth = dateTimeOffset.Month        
        test <@
            match v' with
            | Ok { Year = year; Month = month; Build = Some (Alpha a) } ->
                match v.Build with
                | Some (AlphaNightly (an, _)) ->
                    (int year) = expectedYear &&
                    (int month) = expectedMonth &&
                    a.Number > an.Number                        
                | _ -> false   
            | _ -> false @> 
            
    [<Property( Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.Accidental> |])>]
    let ``BetaNightly prohibits to alpha``  
        (v: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let release = tryAlpha v dateTimeOffset
        test <@
            match release with
            | Error Calaf.Domain.DomainError.BuildDowngradeProhibited -> true
            | _ -> false @>        
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Alpha.Accidental> |])>]
    let ``Alpha CalendarVersion + max alpha Number releases to alpha resets alpha Number to init value the year, month and patch are same``
        (alpha: CalendarVersion) =
        let alpha =
            match alpha.Build with
            | Some (Alpha _) ->
                let build = Alpha { Number = BuildNumber.MaxValue } |> Some
                { alpha with Build = build }
            | _ -> alpha
        let dateTimeOffset = Internals.asDateTimeOffset alpha
        let release = tryAlpha alpha dateTimeOffset
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Alpha a) } ->
                year = alpha.Year &&
                month = alpha.Month &&
                patch = alpha.Patch &&
                a.Number = Calaf.Domain.Build.NumberStartValue
            | _ -> false @>
        
module TryBetaTests =
    [<Fact>]
    let ``Beta short CalendarVersion releases to beta increases beta number but keeps the same Year, Month without Patch when the year and month are same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = None; Build = Some (Build.Beta { Number = 1us }) }
        let dateTimeOffsetStamp = System.DateTime (2023, 10, 31) |> System.DateTimeOffset
        let release = tryBeta calVer dateTimeOffsetStamp
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Beta b) } ->
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                b.Number > 1us
            | _ -> false @>
        
    [<Fact>]
    let ``Beta patch CalendarVersion releases to beta increases beta number but keeps the same Year, Month, Patch when the year, month and patch are same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }
        let dateTimeOffsetStamp = System.DateTime (2023, 10, 31) |> System.DateTimeOffset
        let release = tryBeta calVer dateTimeOffsetStamp
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Beta b) } ->
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                b.Number > 1us
            | _ -> false @>
        
    [<Fact>]
    let ``Stable CalendarVersion releases to beta keeps the same Year, Month, increase Patch and adds Build when the year and month are same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }
        let dateTimeOffsetStamp = System.DateTime(2023, 10, 31) |> System.DateTimeOffset
        let release = tryBeta calVer dateTimeOffsetStamp
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Beta b) } ->
                year = calVer.Year &&
                month = calVer.Month &&
                patch > calVer.Patch &&
                b.Number = Calaf.Domain.Build.NumberStartValue
            | _ -> false @>
        
    [<Fact>]
    let ``BetaNightly CalendarVersion releases to beta switch to beta, increases beta number but keeps the same Year, Month, Patch when the year and month are same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 5us },{ Day = byte 31; Number = 2us } ) ) }
        let dateTimeOffsetStamp = System.DateTime (2023, 10, 31) |> System.DateTimeOffset
        let release = tryBeta calVer dateTimeOffsetStamp
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Beta b) } ->
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                b.Number > 5us
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``Beta CalendarVersion + max beta Number releases to beta resets beta Number to init value the year, month and patch are same``
        (beta: CalendarVersion) =
        let beta =
            match beta.Build with
            | Some (Beta _) ->
                let build = Beta { Number = BuildNumber.MaxValue } |> Some
                { beta with Build = build }
            | _ -> beta
        let dateTimeOffset = Internals.asDateTimeOffset beta
        let release = tryBeta beta dateTimeOffset
        test <@
            match release with
            | Ok { Year = year; Month = month; Patch = patch; Build = Some (Beta b) } ->
                year = beta.Year &&
                month = beta.Month &&
                patch = beta.Patch &&
                b.Number = Calaf.Domain.Build.NumberStartValue
            | _ -> false @> 
      
module TryStableTests =
    [<Fact>]
    let ``Beta CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
        let calVer = { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }        
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        test <@
                 match release with
                 | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
                    year = calVer.Year &&
                    month = calVer.Month &&
                    patch = calVer.Patch &&
                    build.IsNone
                 | _ -> false
             @>
        
    [<Fact>]
    let ``Beta CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
        let calVer = { Year = 9999us; Month = 11uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        
        match release with
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month            
            test <@
               int year = dateTimeOffsetYear &&
               int month = dateTimeOffsetMonth &&
               patch.IsNone &&
               build.IsNone
            @>
        | _ -> test <@ false  @>

        
    [<Fact>]
    let ``Nightly CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
        let calVer = { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        match release with
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->             
            test <@             
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
        let calVer = { Year = 9999us; Month = 11uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset        
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->            
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month            
            test <@ 
                int year = dateTimeOffsetYear &&
                int month = dateTimeOffsetMonth &&
                patch.IsNone &&
                build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Fact>]
    let ``BetaNightly CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
        let calVer = { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 1us }, { Day = 31uy; Number = 1us })) }
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->            
            test <@
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Fact>]
    let ``BetaNightly CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
        let calVer = { Year = 9999us; Month = 11uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 1us }, { Day = 31uy; Number = 1us } )) }
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month            
            test <@                     
                 int year = dateTimeOffsetYear &&
                 int month = dateTimeOffsetMonth &&
                 patch.IsNone &&
                 build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Fact>]
    let ``Stable CalendarVersion when release to stable changes Patch, when the year and month are the same`` () =
        let calVer = { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }        
        let dateTimeOffset = System.DateTime (9999, 12, 31) |> System.DateTimeOffset        
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
            test <@
                year = calVer.Year &&
                month = calVer.Month &&
                patch > calVer.Patch &&
                build.IsNone
            @>
        | _ -> test <@ false @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalPreReleases> |])>]
    let ``Beta/Nightly/BetaNightly CalendarVersion stable releases drops Build and keeps Year, Month, Patch same when the date is same``
        (calVer: CalendarVersion) =
        let dateTimeOffset = System.DateTime (int calVer.Year, int calVer.Month, 28) |> System.DateTimeOffset
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
            test <@
                year = calVer.Year &&
                month = calVer.Month &&
                patch = calVer.Patch &&
                build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalPreReleases> |], MaxTest = 10000)>]
    let ``Alpha/Beta/Nightly/AlphaNightly/BetaNightly CalendarVersion stable releases drops Patch, Build and set Year, Month according to the changes when the date is different``
        (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =        
        let dateTimeOffset = Internals.uniqueDateTimeOffset (calVer, dateTimeOffset)
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            test <@
                int year = dateTimeOffsetYear &&
                int month = dateTimeOffsetMonth &&
                patch.IsNone &&
                build.IsNone
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Stable CalendarVersion releases expected Year`` (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (calVer, dateTimeOffset)
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Year = year } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            test <@
                int year = dateTimeOffsetYear
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``CalendarVersion stable release Month when the DateTimeOffset Month is greater``
        (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (calVer, dateTimeOffset)        
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Month = month } ->
            let dateTimeOffsetMonth = dateTimeOffset.Month
            test <@
                int month = dateTimeOffsetMonth
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Patch> |])>]
    let ``Three-part Stable CalendarVersion stable release only Patch when the DateTimeOffset has the same Year and Month``
        (calVerWithPatch: CalendarVersion) =
        let calVerWithPatch = { calVerWithPatch with Patch = Internals.preventPatchOverflow calVerWithPatch.Patch }
        let dateTimeOffset = Internals.asDateTimeOffset calVerWithPatch
        let release = tryStable calVerWithPatch dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month; Patch = patch } ->
            test <@
                patch > calVerWithPatch.Patch &&
                month = calVerWithPatch.Month &&
                year = calVerWithPatch.Year
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalPatch> |])>]
    let ``CalendarVersion preserves Year and Month when the DateTimeOffset has the same Year and Month``
        (calVerWithPatch: CalendarVersion)=
        let dateTimeOffset = Internals.asDateTimeOffset calVerWithPatch
        let release = tryStable calVerWithPatch dateTimeOffset
        
        match release with                 
        | Ok { Year = year; Month = month } ->
            test <@
                year = calVerWithPatch.Year &&
                month = calVerWithPatch.Month
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalPatch> |])>]
    let ``Three-part CalendarVersion reset Patch when the DateTimeOffset Year and/or Month is different``
        (calVerWithPatch: CalendarVersion, dateTimeOffset: System.DateTimeOffset)=
        let dateTimeOffset = Internals.uniqueDateTimeOffset (calVerWithPatch, dateTimeOffset)
        let release = tryStable calVerWithPatch dateTimeOffset
        
        match release with                 
        | Ok { Patch = patch } ->
            test <@ patch = None @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalShort> |])>]
    let ``Two-part CalendarVersion still has None Patch when the DateTimeOffset Year and/or Month is greater``
        (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (calVer, dateTimeOffset)
        let release = tryStable calVer dateTimeOffset
        
        match release with                 
        | Ok { Patch = patch } ->
            patch = None
        | _ -> false
       
module TryNightlyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Patch> |])>]
    let ``Stable CalendarVersion with Patch releases to nightly and keeps Year, Month, but increases Patch when the year & month are same``
       (stable: CalendarVersion) =
       let stable = { stable with Patch = Internals.preventPatchOverflow stable.Patch }
       let dateTimeOffset = Internals.asDateTimeOffset stable
       let release = tryNightly stable dateTimeOffset
        
       match release with                 
       | Ok { Year = year; Month = month; Patch = patch; Build = build } ->
           test <@
               year = stable.Year &&
               month = stable.Month &&
               patch > stable.Patch &&
               build.Value.IsNightly
           @>
       | _ -> test <@ false @>
         
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Patch> |])>]
    let ``Stable CalendarVersion with max Patch releases to nightly and keeps Year, Month, but resets Patch when the year & month are same``
        (stable: CalendarVersion) =
        let stable = { stable with Patch = Some Patch.MaxValue }
        let dateTimeOffset = Internals.asDateTimeOffset stable
        let release = tryNightly stable dateTimeOffset
        
        test <@
            match release with
            | Ok r ->
                r.Year = stable.Year &&
                r.Month = stable.Month &&
                r.Patch = Some Calaf.Domain.Patch.PatchStartValue &&
                r.Build.Value.IsNightly
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Short> |])>]
    let ``Stable CalendarVersion without Patch releases to nightly and keeps Year, Month, adds Patch when the year & month are same and Patch has max allowed value``
        (stable: CalendarVersion) =
        let stable = { stable with Patch = Some Patch.MaxValue }
        let dateTimeOffset = Internals.asDateTimeOffset stable
        let release = tryNightly stable dateTimeOffset        
        match release with
        | Ok r ->
            test <@
                r.Year = stable.Year &&
                r.Month = stable.Month &&
                r.Patch.IsSome &&
                r.Build.Value.IsNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Patch> |])>]
    let ``Stable CalendarVersion with Patch releases to nightly and sets Year, Month, removes Patch when the year and/or month are different``
        (stable: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (stable, dateTimeOffset)        
        let release = tryNightly stable dateTimeOffset
        
        match release with
        | Ok r ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            
            test <@
                int r.Year = dateTimeOffsetYear &&
                int r.Month = dateTimeOffsetMonth &&
                r.Patch.IsNone &&
                r.Build.Value.IsNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Stable.Short> |])>]
    let ``Stable CalendarVersion without Patch releases to nightly and sets Year, Month, with no Patch when the year and/or month are different``
        (stable: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (stable, dateTimeOffset)
        let release = tryNightly stable dateTimeOffset
        
        match release with
        | Ok r ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            
            test <@
                int r.Year = dateTimeOffsetYear &&
                int r.Month = dateTimeOffsetMonth &&
                r.Patch.IsNone &&
                r.Build.Value.IsNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``Beta CalendarVersion with Patch releases to beta-nightly and keeps Year, Month, Patch when the year & month are same``
        (beta: CalendarVersion) =
        let dateTimeOffset = Internals.asDateTimeOffset beta
        let release = tryNightly beta dateTimeOffset
        
        match release with
        | Ok r ->
            test <@
                r.Year = beta.Year &&
                r.Month = beta.Month &&
                r.Patch = beta.Patch &&
                r.Build.Value.IsBetaNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``Beta CalendarVersion releases to beta-nightly and sets Year, Month, removes Patch when the year and/or month are different``
        (beta: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (beta, dateTimeOffset)
        let release = tryNightly beta dateTimeOffset
        match release with
        | Ok r ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            test <@
                int r.Year = dateTimeOffsetYear &&
                int r.Month = dateTimeOffsetMonth &&
                r.Patch.IsNone &&
                r.Build.Value.IsBetaNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``Beta CalendarVersion releases to beta-nightly and Build keeps Beta Number, has expected Day and init night number when the year and/or month are different``
        (beta: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (beta, dateTimeOffset)
        let release = tryNightly beta dateTimeOffset
        match release with
        | Ok { Build = Some (BetaNightly ({Number = bn}, { Day = nd; Number = nn })) } ->
            let dateTimeOffsetDay = dateTimeOffset.Day            
            test <@
                (Some bn = match beta.Build.Value with | Beta b -> Some b.Number | _ -> None) &&
                int nd = dateTimeOffsetDay &&
                nn = Calaf.Domain.Build.NumberStartValue
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.Accidental> |])>]
    let ``Nightly CalendarVersion releases to nightly and keeps Year, Month, Patch when the year & month are same``
        (n: CalendarVersion) =
        let dateTimeOffset = Internals.asDateTimeOffset n
        let release = tryNightly n dateTimeOffset
        test <@
            match release with
            | Ok r ->
                r.Year = n.Year &&
                r.Month = n.Month &&
                r.Patch = n.Patch &&
                r.Build.Value.IsNightly
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.Patch> |])>]
    let ``Nightly CalendarVersion with Patch releases to nightly and sets Year, Month removes Patch when the year and/or month are different``
        (n: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (n, dateTimeOffset)
        let release = tryNightly n dateTimeOffset
        
        match release with
        | Ok r ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            
            test <@
                int r.Year = dateTimeOffsetYear &&
                int r.Month = dateTimeOffsetMonth &&
                r.Patch.IsNone &&
                r.Build.Value.IsNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.Accidental> |])>]
    let ``Nightly CalendarVersion with max Number releases to nightly and resets Number to Init value when the year, month are same``
        (n: CalendarVersion) =
        let n =
            match n.Build with
            | Some (Nightly { Day = nd }) ->
                let build = Nightly { Day = nd; Number = BuildNumber.MaxValue } |> Some
                { n with Build = build }
            | _ -> n
        let dateTimeOffset = Internals.asDateTimeOffset n
        let release = tryNightly n dateTimeOffset
        
        match release with
        | Ok { Year = year; Month = month; Build = Some (Nightly { Day = nd; Number = nn }) } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            let dateTimeOffsetDay = dateTimeOffset.Day
            
            test <@            
                int year = dateTimeOffsetYear &&
                int month = dateTimeOffsetMonth &&
                int nd = dateTimeOffsetDay &&
                nn = Calaf.Domain.Build.NumberStartValue @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.Accidental> |])>]
    let ``Beta-Nightly CalendarVersion releases to beta-nightly and keeps Year, Month, Patch when the year & month are same``
        (bn: CalendarVersion) =
        let dateTimeOffset = Internals.asDateTimeOffset bn
        let release = tryNightly bn dateTimeOffset       
        
        match release with
        | Ok r ->
            test <@
                r.Year = bn.Year &&
                r.Month = bn.Month &&
                r.Patch = bn.Patch &&
                r.Build.Value.IsBetaNightly @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.Patch> |])>]
    let ``Beta-Nightly CalendarVersion with Patch releases to beta-nightly and sets Year, Month removes Patch when the year and/or month are different``
        (bn: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dateTimeOffset = Internals.uniqueDateTimeOffset (bn, dateTimeOffset)
        let release = tryNightly bn dateTimeOffset
        
        match release with
        | Ok r ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            test <@
                int r.Year = dateTimeOffsetYear &&
                int r.Month = dateTimeOffsetMonth &&
                r.Patch.IsNone &&
                r.Build.Value.IsBetaNightly
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.Accidental> |])>]
    let ``Beta-Nightly CalendarVersion with max nightly Number releases to beta-nightly and resets nightly Number to Init value when the year, month are same``
        (bn: CalendarVersion) =
        let bn =
            match bn.Build with
            | Some (BetaNightly (b, { Day = nd })) ->
                let build = BetaNightly (b, { Day = nd; Number = BuildNumber.MaxValue }) |> Some
                { bn with Build = build }
            | _ -> bn
        let dateTimeOffset = Internals.asDateTimeOffset bn
        
        let release = tryNightly bn dateTimeOffset
        match release with
        | Ok { Year = year; Month = month; Build = (Some (BetaNightly(_, { Day = nd; Number = nn }))) } ->
            let dateTimeOffsetYear = dateTimeOffset.Year
            let dateTimeOffsetMonth = dateTimeOffset.Month
            let dateTimeOffsetDay = dateTimeOffset.Day
            test <@
                int year = dateTimeOffsetYear &&
                int month = dateTimeOffsetMonth &&
                int nd = dateTimeOffsetDay &&
                nn = Calaf.Domain.Build.NumberStartValue
            @>
        | _ -> test <@ false @>
        
module ToStringPropertiesTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``CalendarVersion contains it's Year in the string representation`` (release: CalendarVersion) =
        let releaseString = toString release
        test <@ releaseString.Contains(string release.Year) @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``CalendarVersion contains it's Month in the string representation`` (release: CalendarVersion) =
        let releaseString = toString release
        test <@ releaseString.Contains(string release.Month) @>   
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.AccidentalPatch> |])>]
    let ``CalendarVersion with Patch contains it's Patch in the string representation`` (release: CalendarVersion) =
        let releaseString = toString release
        test <@ releaseString.Contains(string release.Patch.Value) @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Calendar Version's roundtrip. CalendarVersion converts to string and back to the same CalendarVersion`` (release: CalendarVersion) =
        let releaseString = toString release
        let release' = tryParseFromString releaseString
        test <@ (release |> CalVer |> Some) = release' @>
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.Accidental> |])>]
    let ``CalendarVersion with Beta Build contains it's Build in the string representation`` (beta: CalendarVersion) =
        let betaString = toString beta
        test <@
            match beta with
            | { Build = Some (Build.Beta { Number = number }) } ->
                betaString.Contains(Calaf.Domain.Build.BetaBuildType) &&
                betaString.Contains($"{number}")
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.Accidental> |])>]
    let ``CalendarVersion with Nightly Build contains it's Build in the string representation`` (nightly: CalendarVersion) =
        let nightlyString = toString nightly
        test <@
            match nightly with
            | { Build = Some (Build.Nightly { Day = day; Number = number }) } ->
                nightlyString.Contains(Calaf.Domain.Build.NightlyBuildType) &&
                nightlyString.Contains $"{Calaf.Domain.Build.NightlyZeroPrefix}" &&
                nightlyString.Contains(string day) &&
                nightlyString.Contains(string number)
            | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.Accidental> |])>]
    let ``CalendarVersion with BetaNightly Build contains it's Build in the string representation`` (betaNightly: CalendarVersion) =
        let betaNightlyString = toString betaNightly
        test <@
            match betaNightly with
            | { Build = Some (Build.BetaNightly ({ Number = bn }, { Day = nd; Number = nn })) } ->
                betaNightlyString.Contains Calaf.Domain.Build.BetaBuildType &&
                betaNightlyString.Contains $"{bn}" &&
                betaNightlyString.Contains $"{nd}" &&
                betaNightlyString.Contains $"{nn}"
            | _ -> false @>        
  
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Equal CalendarVersion values produce identical strings`` (release: CalendarVersion)=
        let release' = release
        let releaseString  = toString release
        let releaseString' = toString release'
        releaseString = releaseString'
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Output is never empty`` (release: CalendarVersion) =
        release
        |> toString
        |> System.String.IsNullOrWhiteSpace
        |> not
      
module ToTagNamePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Prefix is prepended to the CalendarVersion tag name`` (release: CalendarVersion) =
        let releaseTagName = toTagName release
        releaseTagName.StartsWith tagVersionPrefix    
    
    // Suffix Format
    // The part after the prefix should match the output of toString calVer.
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Tag name after prefix matches CalendarVersion string representation`` (release: CalendarVersion) =
        let releaseTagName = toTagName release
        let versionString = toString release
        releaseTagName.EndsWith versionString
    
    // Reversibility
    // If you strip the prefix from the result and parse it, you should recover the original CalendarVersion (assuming toString and parsing are consistent).
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Tag name can be parsed back to CalendarVersion`` (release: CalendarVersion) =
        let releaseTagName = toTagName release
        test <@
            match tryParseFromTag releaseTagName with
            | Some (CalVer release') -> release' = release
            | _ -> false @>
        
    // No Unexpected Characters
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Tag name does not contain unexpected empty or whitespace characters`` (release: CalendarVersion) =
        let releaseTagName = toTagName release
        test <@
            not (System.String.IsNullOrWhiteSpace releaseTagName) &&
            releaseTagName |> Seq.forall (fun c -> not (System.Char.IsWhiteSpace c)) @>
        
module ToCommitMessagePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Commit message starts with commitVersionPrefix`` (release: CalendarVersion) =
        let commitMessage = toCommitMessage release
        test <@ commitMessage.StartsWith commitVersionPrefix @>
    
     // Suffix Format
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Commit message ends with a string representation of the version`` (release: CalendarVersion) =
        let commitMessage = toCommitMessage release
        test <@ commitMessage.EndsWith (toString release) @>

    // Contains commit version prefix
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Commit message does not contain unexpected empty or whitespace and contains commit version prefix`` (release: CalendarVersion) =
        let commitMessage = toCommitMessage release
        test <@
            System.String.IsNullOrWhiteSpace commitMessage |> not &&
            commitMessage.Contains commitVersionPrefix @>
         
    // Commit string contains 2 whitespace
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Commit message contains two whitespaces on its format`` (release: CalendarVersion) =
        let commitMessage = toCommitMessage release
        let spaceCount = commitMessage |> Seq.filter (fun c -> c = ' ') |> Seq.length
        test <@ spaceCount = 2 @>