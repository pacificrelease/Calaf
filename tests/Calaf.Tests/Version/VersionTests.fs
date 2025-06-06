namespace Calaf.Tests.VersionTests

open FsCheck.Xunit

open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Version
open Calaf.Tests

module TryParseFromStringPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validThreePartCalVerString> |], MaxTest = 200)>]
    let ``Valid three-part CalVer string parses to their corresponding values`` (validVersion: string) =
        let parts = validVersion.Split('.')
        let expectedYear = uint16 parts[0]
        let expectedMonth = byte parts[1]
        let expectedPatch = uint32 parts[2]

        match tryParseFromString validVersion with
        | Some (CalVer calVer) ->
            calVer.Year = expectedYear &&
            calVer.Month = expectedMonth &&
            calVer.Patch = Some expectedPatch
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validTwoPartCalVerString> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to their corresponding values`` (validVersion: string) =
        let parts = validVersion.Split('.')
        let expectedYear = uint16 parts[0]
        let expectedMonth = byte parts[1]

        match tryParseFromString validVersion with
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
    
    [<Property(Arbitrary = [| typeof<Arbitrary.whiteSpaceLeadingTrailingValidCalVerString> |], MaxTest = 200)>]
    let ``Leading/trailing whitespace in input is ignored`` (str: string) =        
        match tryParseFromString str with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validTwoPartCalVerString> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to CalVer with None Patch`` (twoPartCalVerString: string) =
        match tryParseFromString twoPartCalVerString with
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
    [<Property(Arbitrary = [| typeof<Arbitrary.validTagCalVerString> |], MaxTest = 200)>]
    let ``Correct prefix + valid CalVer string parses to Some CalVer`` (validTagCalVerString: string) =
        match tryParseFromTag validTagCalVerString with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.SematicVersion.semanticVersionStr> |], MaxTest = 200)>]
    let ``Correct prefix + valid SemVer string parses to Some SemVer`` (semanticVersionStr: string) =
        match tryParseFromTag semanticVersionStr with
        | Some (SemVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.whiteSpaceLeadingTrailingValidTagCalVerString> |], MaxTest = 200)>]
    let ``Leading/trailing whitespace in input is ignored`` (str: string) =        
        match tryParseFromTag str with
        | Some (CalVer _) -> true
        | _               -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validTwoPartTagCalVerString> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to CalVer with None Patch`` (twoPartCalVerString: string) =
        match tryParseFromTag twoPartCalVerString with
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

    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |], MaxTest = 200)>]
    let ``Single element returns that element`` (version: CalendarVersion) =
        let versions = [| version |]
        versions
        |> tryMax = Some version

    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersions> |], MaxTest = 200)>]
    let ``Multiple elements returns the maximum`` (versions: CalendarVersion[]) =
        versions
        |> tryMax
        |> Option.isSome
      
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersions> |], MaxTest = 200)>]
    let ``Result element belongs to input`` (versions: CalendarVersion[]) =
        let max = versions |> tryMax
        let contains = versions |> Array.contains max.Value
        contains
        
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersions> |], MaxTest = 200)>]
    let ``Order invariance (array reversal does not affect result)`` (versions: CalendarVersion[]) =
        versions
        |> tryMax = tryMax (Array.rev versions)
    
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersions> |])>]
    let ``Duplicate tolerance`` (versions: CalendarVersion[]) =
        let max = tryMax versions
        let duplicated = Array.append versions [| max.Value |]
        tryMax duplicated = max
        
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersions> |])>]
    let ``Result is maximum`` (versions: CalendarVersion[]) =
        let max = versions
                |> tryMax                
        versions
        |> Array.forall (fun v -> compare v max.Value <= 0)
        
module ReleasePropertiesTests =
    let private increment (monthStamp: MonthStamp, incr: MonthStampIncrement ) =
        match incr with
        | Year -> { monthStamp with Year = monthStamp.Year + 1us }
        | Month -> { monthStamp with Month = monthStamp.Month + byte 1 }
        | Both -> { Year = monthStamp.Year + 1us; Month = monthStamp.Month + byte 1 }
    
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersionWithSameMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``CalendarVersion release Year when the MonthStamp Year is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Year = uint16 monthStamp.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersionWithSameMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``CalendarVersion release Month when the MonthStamp Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Month = byte monthStamp.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.threePartCalendarVersionWithSameMonthStamp> |])>]
    let ``Three-part CalendarVersion release only Patch when the MonthStamp has the same Year and Month`` (calVer: CalendarVersion, monthStamp: MonthStamp) =
        let release = release calVer monthStamp
        release.Patch > calVer.Patch &&
        release.Month = calVer.Month &&
        release.Year = calVer.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.threePartCalendarVersionWithSameMonthStamp> |])>]
    let ``CalendarVersion preserves Year and Month when the MonthStamp has the same Year and Month`` (calVer: CalendarVersion, monthStamp: MonthStamp)=
        let release = release calVer monthStamp
        release.Year = calVer.Year &&
        release.Month = calVer.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.threePartCalendarVersionWithSameMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``Three-part CalendarVersion reset Patch when the MonthStamp Year and/or Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement)=
        let timeStamp = increment (monthStamp, incr)
        let release = release calVer timeStamp
        release.Patch = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.twoPartCalendarVersionWithSameMonthStamp>; typeof<Arbitrary.monthStampIncrement> |])>]
    let ``Two-part CalendarVersion still has None Patch when the MonthStamp Year and/or Month is greater`` ((calVer: CalendarVersion, monthStamp: MonthStamp), incr: MonthStampIncrement) =
        let monthStamp = increment (monthStamp, incr)
        let release = release calVer monthStamp
        release.Patch = None
        
module ToStringPropertiesTests =
    let private dotSegments (s:string) = s.Split '.'
    
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``CalendarVersion contains it's Year in the string representation`` (calVer: CalendarVersion) =
        let calVerString = calVer |> toString
        calVerString.Contains(calVer.Year |> string)
    
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``CalendarVersion contains it's Month in the string representation`` (calVer: CalendarVersion) =
        let calVerString = calVer |> toString
        calVerString.Contains(calVer.Month |> string)   
        
    [<Property(Arbitrary = [| typeof<Arbitrary.threeSectionCalendarVersion> |])>]
    let ``CalendarVersion with Patch contains it's Patch in the string representation`` (calVer: CalendarVersion) =
        let calVerString = calVer |> toString
        calVerString.Contains(calVer.Patch.Value |> string)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.threeSectionCalendarVersion> |])>]
    let ``CalendarVersion with Patch contains three string sections`` (calVer: CalendarVersion) =
        calVer
        |> toString
        |> dotSegments
        |> Array.length  = 3
        
    [<Property(Arbitrary = [| typeof<Arbitrary.twoSectionCalendarVersion> |])>]
    let ``CalendarVersion without Patch contains two string sections`` (calVer: CalendarVersion) =
        calVer
        |> toString
        |> dotSegments
        |> Array.length  = 2
        
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Equal CalendarVersion values produce identical strings`` (calVer: CalendarVersion)=
        let copy = calVer
        let calVerString1 = calVer |> toString
        let calVerString2 = copy   |> toString
        calVerString1 = calVerString2
        
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Output is never empty`` (calVer: CalendarVersion) =
        calVer
        |> toString
        |> System.String.IsNullOrWhiteSpace
        |> not
        
module ToTagNamePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Prefix is prepended to the CalendarVersion tag name`` (calVer: CalendarVersion) =
        let tag = calVer |> toTagName
        tag.StartsWith(tagVersionPrefix)    
    
    // Suffix Format
    // The part after the prefix should match the output of toString calVer.
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Tag name after prefix matches CalendarVersion string representation`` (calVer: CalendarVersion) =
        let tag = calVer |> toTagName
        let versionString = calVer |> toString
        tag.EndsWith(versionString)
    
    // Reversibility
    // If you strip the prefix from the result and parse it, you should recover the original CalendarVersion (assuming toString and parsing are consistent).
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Tag name can be parsed back to CalendarVersion`` (calVer: CalendarVersion) =
        let tagName = calVer |> toTagName
        match tryParseFromTag tagName with
        | Some (CalVer version) -> version = calVer
        | _ -> false
        
    // No Unexpected Characters
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Tag name does not contain unexpected empty or whitespace characters`` (calVer: CalendarVersion) =
        let tagName = calVer |> toTagName
        not (System.String.IsNullOrWhiteSpace tagName) &&
        tagName |> Seq.forall (fun c -> not (System.Char.IsWhiteSpace c))
        
module ToCommitMessagePropertiesTests =
    // Prefix Prepending
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Commit message starts with commitVersionPrefix`` (calVer: CalendarVersion) =
        let commitMsg = calVer |> toCommitMessage
        commitMsg.StartsWith(commitVersionPrefix)
    
    // Suffix Format
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Commit message ends with string representation of version`` (calVer: CalendarVersion) =
        let commitMsg = calVer |> toCommitMessage
        commitMsg.EndsWith(toString calVer)

    // Contains commit version prefix
    [<Property(Arbitrary = [| typeof<Arbitrary.calendarVersion> |])>]
    let ``Commit message does not contain unexpected empty or whitespace and contains commit version prefix`` (calVer: CalendarVersion) =
        let commitMsg = calVer |> toCommitMessage
        not (System.String.IsNullOrWhiteSpace commitMsg) &&
        commitMsg.Contains commitVersionPrefix