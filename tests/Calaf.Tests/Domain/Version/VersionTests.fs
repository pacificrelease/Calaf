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
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.String> |])>]
    let ``Beta calendar version string always parses to the CalendarVersion with Beta Build`` (betaString: string) =        
        let version = tryParseFromString betaString
        let hasBetaBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsBeta | _ -> false)
            |> Option.defaultValue false
        test <@ hasBetaBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.String> |])>]
    let ``Nightly calendar version string always parses to the CalendarVersion with Nightly Build`` (nightlyString: string) =        
        let version = tryParseFromString nightlyString
        let hasNightlyBetaBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsNightly | _ -> false)
            |> Option.defaultValue false
        test <@ hasNightlyBetaBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.String> |])>]
    let ``Beta-Nightly calendar version string always parses to the CalendarVersion with BetaNightly Build`` (betaNightlyString: string) =        
        let version = tryParseFromString betaNightlyString
        let hasBetaNightlyBetaBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsBetaNightly | _ -> false)
            |> Option.defaultValue false
        test <@ hasBetaNightlyBetaBuild @>
        
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
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Beta.TagString> |])>]
    let ``Beta Calendar Version tag string always parses to Calendar Version with Beta Build`` (tag: string) =         
        let version = tryParseFromTag tag
        let hasBetaBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsBeta | _ -> false)
            |> Option.defaultValue false
        test <@ hasBetaBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Nightly.TagString> |])>]
    let ``Nightly Calendar Version tag string always parses to Calendar Version with Nightly Build`` (tag: string) =         
        let version = tryParseFromTag tag
        let hasNightlyBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsNightly | _ -> false)
            |> Option.defaultValue false
        test <@ hasNightlyBuild @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.BetaNightly.TagString> |])>]
    let ``Beta-Nightly Calendar Version tag string always parses to Calendar Version with BetaNightly Build`` (tag: string) =         
        let version = tryParseFromTag tag
        let hasBetaNightlyBuild =
            version
            |> Option.map (function | CalVer v -> v.Build.Value.IsBetaNightly | _ -> false)
            |> Option.defaultValue false
        test <@ hasBetaNightlyBuild @>
        
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
    let ``Same date stable, beta, nightly, beta-nightly versions return beta-nightly version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
        ]
        let expected = versions[4] |> Some
            
        let max = versions |> tryMax        
        test <@ expected = max @>
        
    [<Fact>]
    let ``Same date stable, beta, nightly, beta-nightly + higher beta versions return higher beta version`` () =
        let versions = [
            { Year = 9999us; Month = 12uy; Patch = None; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = None }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Nightly { Day = 10uy; Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 1us }) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (BetaNightly ({ Number = 1us }, { Day = 10uy; Number = 1us })) }
            { Year = 9999us; Month = 12uy; Patch = Some 1u; Build = Some (Beta { Number = 2us }) }
        ]
        let expected = versions[5] |> Some
            
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

// module BetaTests =
//     [<Fact>]
//     let ``Beta CalendarVersion releases to beta uncreases beta number but keeps the same Year, Month, Patch when the year and month are same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }
//         let dateTimeOffsetStamp = System.DateTime (2023, 10, 31) |> System.DateTimeOffset
//         let release = beta calVer dateTimeOffsetStamp
//         let betaNumber = match release.Build.Value with | Beta b -> Some b.Number | _ -> None
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && betaNumber > (Some 1us) @>
//         
//     [<Fact>]
//     let ``Stable CalendarVersion releases to beta keeps the same Year, Month, increase Patch and adds Build when the year and month are same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }
//         let dateTimeOffsetStamp = System.DateTime(2023, 10, 31) |> System.DateTimeOffset
//         let release = beta calVer dateTimeOffsetStamp
//         let betaNumber = match release.Build.Value with | Beta b -> Some b.Number | _ -> None
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch > calVer.Patch && betaNumber = (Some Calaf.Domain.Build.NumberStartValue) @>
//         
//     [<Fact>]
//     let ``BetaNightly CalendarVersion releases to beta switch to beta, uncreases beta number but keeps the same Year, Month, Patch when the year and month are same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 5us },{ Day = byte 31; Number = 2us } ) ) }
//         let dateTimeOffsetStamp = System.DateTime (2023, 10, 31) |> System.DateTimeOffset
//         let release = beta calVer dateTimeOffsetStamp
//         let betaNumber = match release.Build.Value with | Beta b -> Some b.Number | _ -> None
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && betaNumber > (Some 5us) @>
      
// module StableTests =
//     [<Fact>]
//     let ``Beta CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``Beta CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Beta { Number = 1us }) }
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``BetaNightly CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 1us }, { Day = 31uy; Number = 1us })) }
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``BetaNightly CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.BetaNightly ({ Number = 1us }, { Day = 31uy; Number = 1us } )) }
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
//         
//     [<Fact>]
//     let ``Stable CalendarVersion when release to stable changes Patch, when the year and month are the same`` () =
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }        
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch > calVer.Patch && release.Build.IsNone @>        
//     
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Stable CalendarVersion releases expected Year`` (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
//         let release = stable calVer monthStamp
//         release.Year = uint16 monthStamp.Year
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``CalendarVersion stable release Month when the MonthStamp Month is greater``
//         (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
//         let release = stable calVer monthStamp
//         release.Month = byte monthStamp.Month
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
//     let ``Three-part CalendarVersion stable release only Patch when the MonthStamp has the same Year and Month`` (calVerWithPatch: CalendarVersion) =
//         let monthStamp = { Year = calVerWithPatch.Year; Month = calVerWithPatch.Month }
//         let release = stable calVerWithPatch monthStamp
//         release.Patch > calVerWithPatch.Patch &&
//         release.Month = calVerWithPatch.Month &&
//         release.Year = calVerWithPatch.Year
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
//     let ``CalendarVersion preserves Year and Month when the MonthStamp has the same Year and Month`` (calVerWithPatch: CalendarVersion)=
//         let monthStamp = { Year = calVerWithPatch.Year; Month = calVerWithPatch.Month }
//         let release = stable calVerWithPatch monthStamp
//         release.Year = calVerWithPatch.Year &&
//         release.Month = calVerWithPatch.Month
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
//     let ``Three-part CalendarVersion reset Patch when the MonthStamp Year and/or Month is different`` (calVerWithPatch: CalendarVersion, dateTimeOffset: System.DateTimeOffset)=
//         let monthStamp = Internals.uniqueMonthStamp (calVerWithPatch, dateTimeOffset)
//         let release = stable calVerWithPatch monthStamp
//         release.Patch = None
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShort> |])>]
//     let ``Two-part CalendarVersion still has None Patch when the MonthStamp Year and/or Month is greater`` (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
//         let release = stable calVer monthStamp
//         release.Patch = None
       
// module NightlyTests =   
//     [<Fact>]
//     let ``Stable CalendarVersion when release to nightly keeps the same Year, Month, increase Patch and adds Build when the year and month are the same`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }
//         let dayOfMonth = 31uy
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch > calVer.Patch && release.Build.IsSome @>
//         
//     [<Fact>]
//     let ``Stable CalendarVersion when release to nightly keeps the same Year, Month, adds Patch and adds Build when the year and month are the same and Patch is none`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = None; Build = None }
//         let dayOfMonth = 7uy
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch.IsSome && release.Build.IsSome @>
//     
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly changes month and remove patch and keeps the same Year when the year is the same but month is different`` () =        
//         let calVer = { Year = 2025us; Month = 6uy; Patch = Some 4u; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
//         let dayOfMonth = 2uy
//         let monthStamp = { Year = 2025us; Month = 8uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.Value.IsNightly @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly changes year, month and remove patch when the year is different`` () =        
//         let calVer = { Year = 2024us; Month = 1uy; Patch = Some 10u; Build = Some (Build.Nightly { Day = 7uy; Number = 2us }) }
//         let dayOfMonth = 15uy
//         let monthStamp = { Year = 2025us; Month = 7uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.Value.IsNightly @>
//             
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly keeps the same Year, Month, the same Patch, the same Day and increases Build's Number when the year, month, patch, day are the same`` () =
//         let dayOfMonth = 31uy
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = dayOfMonth; Number = 1us }) }        
//         let monthStamp = { Year = 2023us; Month = 10uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly changes day/number and keeps the same Patch when the same Year, Month, Patch and the day is differed`` () =        
//         let calVer = { Year = 2023us; Month = 11uy; Patch = Some 4u; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
//         let dayOfMonth = 2uy
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly adds new Patch when the same Year, Month, None Patch and the day is differed`` () =        
//         let calVer = { Year = 2023us; Month = 11uy; Patch = None; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
//         let dayOfMonth = 2uy
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to nightly keeps the same Year, Month, Day and increases Build's Number when the year, month, day are the same`` () =        
//         let calVer = { Year = 2023us; Month = 11uy; Patch = None; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
//         let dayOfMonth = 1uy
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = nightly calVer (dayOfMonth, monthStamp)
//         test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
//         
//     [<Fact>]
//     let ``Nightly CalendarVersion when release to stable change the Year, Month and removes Patch when the year and month are differed`` () =        
//         let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
//         let monthStamp = { Year = 2023us; Month = 11uy }
//         let release = stable calVer monthStamp
//         test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
//         
//     //Year Rollover
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``The calendar version Nightly returns version with the MonthStamp Year when the MonthStamp Year is different from the calendar version Year``
//         (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let dayOfMonth = byte dateTimeOffset.Day
//         let monthStamp =
//          if uint16 dateTimeOffset.Year <> calendarVersion.Year
//           then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
//           else { Year = uint16 (dateTimeOffset.AddYears(1).Year); Month = byte dateTimeOffset.Month }          
//         let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
//         test <@ nightly.Year = monthStamp.Year @>
//      
//     // Month Rollover
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``The calendar version Nightly returns version with the MonthStamp Month when the MonthStamp Month is different from the calendar version Month``
//         (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let dayOfMonth = byte dateTimeOffset.Day
//         let monthStamp =
//          if byte dateTimeOffset.Month <> calendarVersion.Month
//           then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
//           else { Year = uint16 dateTimeOffset.Year; Month = byte (dateTimeOffset.AddMonths(1).Month) }
//         let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
//         test <@ nightly.Month = monthStamp.Month @>
//         
//     // Build is always updated
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``The calendar version Nightly always updates its build``
//         (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let dayOfMonth = byte dateTimeOffset.Day
//         let monthStamp = { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
//         let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
//         test <@ calendarVersion.Build <> nightly.Build @>    
//     
//     // Build Day is correct
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``The calendar version Nightly returns a build with the correct day of month``
//         (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         // prepare data
//         let dayOfMonth =
//             if calendarVersion.Build
//                |> Option.map (function
//                     | Build.BetaNightly (_, { Day = day }) -> day = byte dateTimeOffset.Day
//                     | Build.Nightly { Day = day } -> day = byte dateTimeOffset.Day
//                     | _ -> false)
//                |> Option.defaultValue false
//             then byte (dateTimeOffset.AddDays(1).Day)
//             else byte dateTimeOffset.Day
//         let monthStamp = { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
//         let nightly = nightly calendarVersion (dayOfMonth, monthStamp)
//         test <@ nightly.Build
//                 |> Option.map (function
//                     | Build.BetaNightly (_, { Day = day }) -> day = dayOfMonth
//                     | Build.Nightly { Day = day } -> day = dayOfMonth
//                     | _ -> false)
//                 |> Option.defaultValue false @>    
//     
//     // Patch is reset
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``The calendar version Nightly resets Patch when the MonthStamp Year and/or Month is different from the calendar version``
//         (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
//         let dayOfMonth = byte dateTimeOffset.Day
//         let monthStamp =
//          if uint16 dateTimeOffset.Year <> calendarVersion.Year ||
//             byte dateTimeOffset.Month <> calendarVersion.Month            
//           then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
//           else { Year = uint16 (dateTimeOffset.AddYears(1).Year); Month = byte (dateTimeOffset.AddMonths(1).Month) }
//         let nightly = nightly calendarVersion (dayOfMonth, monthStamp)
//         test <@ nightly.Patch.IsNone @>
        
// module ToStringPropertiesTests =
//     let private dotSegments (s:string) = s.Split '.'
//     
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``CalendarVersion contains it's Year in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         calVerString.Contains(calendarVersion.Year |> string)
//     
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``CalendarVersion contains it's Month in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         calVerString.Contains(calendarVersion.Month |> string)   
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
//     let ``CalendarVersion with Patch contains it's Patch in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         calVerString.Contains(calendarVersion.Patch.Value |> string)
//       
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortNightlyBuild> |])>]
//     let ``CalendarVersion with Nightly Build contains it's Build in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         let contains = match calendarVersion with
//                         | { Build = Some (Build.Nightly { Day = day; Number = number }) } ->
//                             calVerString.Contains($"{day}") &&
//                             calVerString.Contains($"{number}")
//                         | _ -> failwith "CalendarVersion with Build should have a Build"
//         test <@ contains @>
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortBetaBuild> |])>]
//     let ``CalendarVersion with Beta Build contains it's Build in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         let contains = match calendarVersion with
//                         | { Build = Some (Build.Beta { Number = number }) } ->                            
//                             calVerString.Contains($"{number}")
//                         | _ -> failwith "CalendarVersion with Build should have a Build"
//         test <@ contains @>
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortBetaNightlyBuild> |])>]
//     let ``CalendarVersion with BetaNightly Build contains it's Build in the string representation`` (calendarVersion: CalendarVersion) =
//         let calVerString = calendarVersion |> toString
//         let contains = match calendarVersion with
//                         | { Build = Some (Build.BetaNightly ({ Number = bn }, { Day = nd; Number = nn })) } ->
//                             calVerString.Contains($"{bn}") &&
//                             calVerString.Contains($"{nd}") &&
//                             calVerString.Contains($"{nn}")
//                         | _ -> failwith "CalendarVersion with Build should have a Build"
//         test <@ contains @>
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
//     let ``CalendarVersion with Patch contains three string sections`` (calendarVersion: CalendarVersion) =
//         calendarVersion
//         |> toString
//         |> dotSegments
//         |> Array.length  = 3
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShort> |])>]
//     let ``CalendarVersion without Patch contains two string sections`` (calendarVersion: CalendarVersion) =
//         calendarVersion
//         |> toString
//         |> dotSegments
//         |> Array.length  = 2
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Equal CalendarVersion values produce identical strings`` (calendarVersion: CalendarVersion)=
//         let copy = calendarVersion
//         let calVerString1 = calendarVersion |> toString
//         let calVerString2 = copy   |> toString
//         calVerString1 = calVerString2
//         
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Output is never empty`` (calendarVersion: CalendarVersion) =
//         calendarVersion
//         |> toString
//         |> System.String.IsNullOrWhiteSpace
//         |> not
      
// module ToTagNamePropertiesTests =
//     // Prefix Prepending
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Prefix is prepended to the CalendarVersion tag name`` (calendarVersion: CalendarVersion) =
//         let tag = calendarVersion |> toTagName
//         tag.StartsWith(tagVersionPrefix)    
//     
//     // Suffix Format
//     // The part after the prefix should match the output of toString calVer.
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Tag name after prefix matches CalendarVersion string representation`` (calendarVersion: CalendarVersion) =
//         let tag = calendarVersion |> toTagName
//         let versionString = calendarVersion |> toString
//         tag.EndsWith(versionString)
//     
//     // Reversibility
//     // If you strip the prefix from the result and parse it, you should recover the original CalendarVersion (assuming toString and parsing are consistent).
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Tag name can be parsed back to CalendarVersion`` (calendarVersion: CalendarVersion) =
//         let tagName = calendarVersion |> toTagName
//         match tryParseFromTag tagName with
//         | Some (CalVer version) -> version = calendarVersion
//         | _ -> false
//         
//     // No Unexpected Characters
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Tag name does not contain unexpected empty or whitespace characters`` (calendarVersion: CalendarVersion) =
//         let tagName = calendarVersion |> toTagName
//         not (System.String.IsNullOrWhiteSpace tagName) &&
//         tagName |> Seq.forall (fun c -> not (System.Char.IsWhiteSpace c))
//         
// module ToCommitMessagePropertiesTests =
//     // Prefix Prepending
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Commit message starts with commitVersionPrefix`` (calendarVersion: CalendarVersion) =
//         let commitMsg = calendarVersion |> toCommitMessage
//         commitMsg.StartsWith(commitVersionPrefix)
//     
//     // Suffix Format
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Commit message ends with string representation of version`` (calendarVersion: CalendarVersion) =
//         let commitMsg = calendarVersion |> toCommitMessage
//         commitMsg.EndsWith(toString calendarVersion)
//
//     // Contains commit version prefix
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Commit message does not contain unexpected empty or whitespace and contains commit version prefix`` (calendarVersion: CalendarVersion) =
//         let commitMsg = calendarVersion |> toCommitMessage
//         not (System.String.IsNullOrWhiteSpace commitMsg) &&
//         commitMsg.Contains commitVersionPrefix
//         
//     // Commit string contains 2 whitespace
//     [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
//     let ``Commit message contains two whitespaces on its format`` (calendarVersion: CalendarVersion) =
//         let commitMsg = toCommitMessage calendarVersion
//         let spaceCount = commitMsg |> Seq.filter (fun c -> c = ' ') |> Seq.length
//         test <@ spaceCount = 2 @>