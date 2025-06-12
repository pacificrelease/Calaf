namespace Calaf.Tests.VersionTests

open FsCheck.Xunit
open Xunit
open Swensen.Unquote

open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Version
open Calaf.Tests

module TryParseFromStringPropertiesTests =
    [<Fact>]
    let ``Nightly CalVer version with patch string parses to it corresponding values`` () =
        //let version = "2023.10"
        let version = "2023.10-nightly.06"
        //let version = "2023.10-nightly.06+0fefe3f"
        //let version = "2023.10-nightly.06+0fefe3fnightly.06+0fefe3f"
        //let version = "2023.10.1-nightly.06+0fefe3f"
        
        //let version = "2023.10.1"
        //let version = "2023.10.1-nightly"        
        
        //let version = "2023.10.1--nightly--."
        //let version = "2023.10.1-nightly.--..2023.10.1--nightly--"
        
        
        
        let version = version |> tryParseFromString
        test <@ version |> Option.map _.IsCalVer |> Option.defaultValue false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatchStr> |], MaxTest = 200)>]
    let ``Release calendar version string parses to the corresponding values`` (releaseVersion: string) =
        let parts = releaseVersion.Split('.')
        let expectedYear = uint16 parts[0]
        let expectedMonth = byte parts[1]
        let expectedPatch = uint32 parts[2]
        
        let version = tryParseFromString releaseVersion
        test <@ match version with | Some (CalVer calVer)
                                    -> calVer.Year = expectedYear &&
                                       calVer.Month = expectedMonth &&
                                       calVer.Patch = Some expectedPatch
                                    | _ -> false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortStr> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to their corresponding values`` (calendarVersionShortStr: string) =
        let parts = calendarVersionShortStr.Split('.')
        let expectedYear = uint16 parts[0]
        let expectedMonth = byte parts[1]

        match tryParseFromString calendarVersionShortStr with
        | Some (CalVer calVer) ->
            calVer.Year = expectedYear &&
            calVer.Month = expectedMonth &&
            calVer.Patch = None            
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.SematicVersion.semanticVersionStr> |], MaxTest = 500)>]
    let ``Valid SemVer string parses to LooksLikeSemVer`` (semanticVersionStr: string) =
        let parts = semanticVersionStr.Split('.')
        let expectedMajor = uint32 parts[0]
        let expectedMinor = uint32 parts[1]
        let expectedPatch = uint32 parts[2]

        match tryParseFromString semanticVersionStr with
        | Some (SemVer semVer) ->
            semVer.Major = expectedMajor &&
            semVer.Minor = expectedMinor &&
            semVer.Patch = expectedPatch
        | _ -> false        
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.whiteSpaceLeadingTrailingCalendarVersionStr> |], MaxTest = 200)>]
    let ``Leading/trailing whitespace in input is ignored`` (whiteSpaceLeadingTrailingCalendarVersionStr: string) =        
        match tryParseFromString whiteSpaceLeadingTrailingCalendarVersionStr with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortStr> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to CalVer with None Patch`` (calendarVersionShortStr: string) =
        match tryParseFromString calendarVersionShortStr with
        | Some (CalVer calVer) -> calVer.Patch.IsNone
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.invalidThreePartString> |], MaxTest = 200)>]
    let ``Invalid CalVer/SemVer but valid three-part format parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to None`` (badVersion: string) =
        badVersion |> tryParseFromString = None
        
module TryParseFromTagPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionTagStr> |], MaxTest = 200)>]
    let ``Correct prefix + valid CalVer string parses to Some CalVer`` (calendarVersionTagStr: string) =
        match tryParseFromTag calendarVersionTagStr with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.SematicVersion.semanticVersionStr> |], MaxTest = 200)>]
    let ``Correct prefix + valid SemVer string parses to Some SemVer`` (semanticVersionStr: string) =
        match tryParseFromTag semanticVersionStr with
        | Some (SemVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.whiteSpaceLeadingTrailingCalendarVersionTagStr> |], MaxTest = 200)>]
    let ``Leading/trailing whitespace in input is ignored`` (tagStr: string) =        
        match tryParseFromTag tagStr with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortTagStr> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to CalVer with None Patch`` (tagStr: string) =
        match tryParseFromTag tagStr with
        | Some (CalVer calVer) -> calVer.Patch.IsNone
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.invalidThreePartString> |], MaxTest = 200)>]
    let ``Invalid CalVer/SemVer but valid three-part format parses to Unsupported`` (invalidVersion: string) =
        invalidVersion |> tryParseFromString = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |], MaxTest = 200)>]
    let ``Null or empty or whitespace string parses to None`` (badVersion: string) =
        badVersion |> tryParseFromTag = None
        
module TryMaxPropertiesTests =
    [<Property(MaxTest = 200)>]
    let ``Empty array returns None`` () =
        let versions = [||]
        versions
        |> tryMax = None

    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |], MaxTest = 200)>]
    let ``Single element returns that element`` (calendarVersion: CalendarVersion) =
        let versions = [| calendarVersion |]
        versions
        |> tryMax = Some calendarVersion

    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |], MaxTest = 200)>]
    let ``Multiple elements returns the maximum`` (calendarVersions: CalendarVersion[]) =
        calendarVersions
        |> tryMax
        |> Option.isSome
      
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |], MaxTest = 200)>]
    let ``Result element belongs to input`` (calendarVersions: CalendarVersion[]) =
        let max = calendarVersions |> tryMax
        let contains = calendarVersions |> Array.contains max.Value
        contains
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |], MaxTest = 200)>]
    let ``Order invariance (array reversal does not affect result)`` (calendarVersions: CalendarVersion[]) =
        calendarVersions
        |> tryMax = tryMax (Array.randomShuffle calendarVersions)
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |])>]
    let ``Duplicate tolerance`` (calendarVersions: CalendarVersion[]) =
        let max = tryMax calendarVersions
        let duplicated = Array.append calendarVersions [| max.Value |]
        tryMax duplicated = max
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |])>]
    let ``Result is maximum`` (calendarVersions: CalendarVersion[]) =
        let max = calendarVersions
                |> tryMax                
        calendarVersions
        |> Array.forall (fun v -> compare v max.Value <= 0)
        
module ReleasePropertiesTests =
    let private increment (monthStamp: MonthStamp, incr: MonthStampIncrement ) =
        match incr with
        | Year -> { monthStamp with Year = monthStamp.Year + 1us }
        | Month -> { monthStamp with Month = monthStamp.Month + byte 1 }
        | Both -> { Year = monthStamp.Year + 1us; Month = monthStamp.Month + byte 1 }
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``CalendarVersion release Year when the MonthStamp Year is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Year = uint16 monthStamp.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``CalendarVersion release Month when the MonthStamp Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Month = byte monthStamp.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatchMonthStamp> |])>]
    let ``Three-part CalendarVersion release only Patch when the MonthStamp has the same Year and Month`` (calVer: CalendarVersion, monthStamp: MonthStamp) =
        let release = release calVer monthStamp
        release.Patch > calVer.Patch &&
        release.Month = calVer.Month &&
        release.Year = calVer.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatchMonthStamp> |])>]
    let ``CalendarVersion preserves Year and Month when the MonthStamp has the same Year and Month`` (calVer: CalendarVersion, monthStamp: MonthStamp)=
        let release = release calVer monthStamp
        release.Year = calVer.Year &&
        release.Month = calVer.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatchMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``Three-part CalendarVersion reset Patch when the MonthStamp Year and/or Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement)=
        let timeStamp = increment (monthStamp, incr)
        let release = release calVer timeStamp
        release.Patch = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``Two-part CalendarVersion still has None Patch when the MonthStamp Year and/or Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Patch = None
        
module ToStringPropertiesTests =
    let private dotSegments (s:string) = s.Split '.'
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``CalendarVersion contains it's Year in the string representation`` (calendarVersion: CalendarVersion) =
        let calVerString = calendarVersion |> toString
        calVerString.Contains(calendarVersion.Year |> string)
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``CalendarVersion contains it's Month in the string representation`` (calendarVersion: CalendarVersion) =
        let calVerString = calendarVersion |> toString
        calVerString.Contains(calendarVersion.Month |> string)   
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
    let ``CalendarVersion with Patch contains it's Patch in the string representation`` (calendarVersion: CalendarVersion) =
        let calVerString = calendarVersion |> toString
        calVerString.Contains(calendarVersion.Patch.Value |> string)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
    let ``CalendarVersion with Patch contains three string sections`` (calendarVersion: CalendarVersion) =
        calendarVersion
        |> toString
        |> dotSegments
        |> Array.length  = 3
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShort> |])>]
    let ``CalendarVersion without Patch contains two string sections`` (calendarVersion: CalendarVersion) =
        calendarVersion
        |> toString
        |> dotSegments
        |> Array.length  = 2
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Equal CalendarVersion values produce identical strings`` (calendarVersion: CalendarVersion)=
        let copy = calendarVersion
        let calVerString1 = calendarVersion |> toString
        let calVerString2 = copy   |> toString
        calVerString1 = calVerString2
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Output is never empty`` (calendarVersion: CalendarVersion) =
        calendarVersion
        |> toString
        |> System.String.IsNullOrWhiteSpace
        |> not
        
module ToTagNamePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Prefix is prepended to the CalendarVersion tag name`` (calendarVersion: CalendarVersion) =
        let tag = calendarVersion |> toTagName
        tag.StartsWith(tagVersionPrefix)    
    
    // Suffix Format
    // The part after the prefix should match the output of toString calVer.
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Tag name after prefix matches CalendarVersion string representation`` (calendarVersion: CalendarVersion) =
        let tag = calendarVersion |> toTagName
        let versionString = calendarVersion |> toString
        tag.EndsWith(versionString)
    
    // Reversibility
    // If you strip the prefix from the result and parse it, you should recover the original CalendarVersion (assuming toString and parsing are consistent).
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Tag name can be parsed back to CalendarVersion`` (calendarVersion: CalendarVersion) =
        let tagName = calendarVersion |> toTagName
        match tryParseFromTag tagName with
        | Some (CalVer version) -> version = calendarVersion
        | _ -> false
        
    // No Unexpected Characters
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Tag name does not contain unexpected empty or whitespace characters`` (calendarVersion: CalendarVersion) =
        let tagName = calendarVersion |> toTagName
        not (System.String.IsNullOrWhiteSpace tagName) &&
        tagName |> Seq.forall (fun c -> not (System.Char.IsWhiteSpace c))
        
module ToCommitMessagePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Commit message starts with commitVersionPrefix`` (calendarVersion: CalendarVersion) =
        let commitMsg = calendarVersion |> toCommitMessage
        commitMsg.StartsWith(commitVersionPrefix)
    
    // Suffix Format
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Commit message ends with string representation of version`` (calendarVersion: CalendarVersion) =
        let commitMsg = calendarVersion |> toCommitMessage
        commitMsg.EndsWith(toString calendarVersion)

    // Contains commit version prefix
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Commit message does not contain unexpected empty or whitespace and contains commit version prefix`` (calendarVersion: CalendarVersion) =
        let commitMsg = calendarVersion |> toCommitMessage
        not (System.String.IsNullOrWhiteSpace commitMsg) &&
        commitMsg.Contains commitVersionPrefix