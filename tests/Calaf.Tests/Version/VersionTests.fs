namespace Calaf.Tests.VersionTests

open FsCheck.Xunit
open Xunit
open Swensen.Unquote

open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Version
open Calaf.Tests

module TryParseFromStringTests =
    [<Fact>]
    let ``Nightly CalVer version with patch string parses to it corresponding values`` () =
        let version = $@"2023.10{CalendarVersionBuildTypeDivider}nightly.31.1"        
        let version = version |> tryParseFromString
        test <@ version |> Option.map _.IsCalVer |> Option.defaultValue false @>
       
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionNightlyBuildStr> |], MaxTest = 200)>]
    let ``Nightly calendar version string parses to the corresponding values`` (nightlyVersion: string) =        
        let version = tryParseFromString nightlyVersion
        test <@ version |> Option.map (function | CalVer v -> v.Build.IsSome | _ -> false) |> Option.defaultValue false @>
        
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
        
module TryParseFromTagTests =   
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
        
module TryMaxTests =
    [<Fact>]
    let ``Same date release, nightly versions return nightly build with higher number and hash (hash may be sorted alphabetically)`` () =
        let versions = [
            { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }
            { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 30uy; Number = 100us }) }
            { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us   }) }
            { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us   }) }
            { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 2us   }) }
            { Year = 2023us; Month = 11uy; Patch = Some 2u; Build = Some (Build.Nightly { Day = 31uy; Number = 3us   }) }
            { Year = 2023us; Month = 12uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us   }) }
        ]
        let max = versions |> tryMax
        test <@ max |> Option.map (fun v -> v = versions[6]) |> Option.defaultValue false @>
        
    [<Fact>]
    let ``Different release and nightly calendar versions return max expected value`` () =
        let versions = 
            [| { Year = 2024us; Month = 11uy; Patch = Some 10u; Build = None }
               { Year = 2024us; Month = 11uy; Patch = Some 11u; Build = Some (Build.Nightly { Day = 31uy; Number = 35us }) }
               { Year = 2025us; Month = 11uy; Patch = None;     Build = None }
               { Year = 2025us; Month = 10uy; Patch = Some 10u; Build = None }
               { Year = 2025us; Month = 10uy; Patch = Some 10u; Build = Some (Build.Nightly { Day = 30uy; Number = 29us }) }
               { Year = 2025us; Month = 10uy; Patch = Some 10u; Build = Some (Build.Nightly { Day = 31uy; Number = 30us }) }
            |]        
        let max = versions |> tryMax
        test <@ max |> Option.map (fun v -> v = versions[2]) |> Option.defaultValue false @>
    
    [<Property>]
    let ``Empty array returns None`` () =
        let versions = [||]
        versions
        |> tryMax = None

    [<Property>]
    let ``Single element returns this element`` (calendarVersion: CalendarVersion) =
        let versions = [| calendarVersion |]
        let max = versions |> tryMax
        test <@ max = Some calendarVersion @>

    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |])>]
    let ``Multiple elements returns the element of this sequence`` (calendarVersions: CalendarVersion[]) =
        let contains =
            calendarVersions
            |> tryMax
            |> Option.map (fun v -> calendarVersions |> Seq.contains v)
            |> Option.defaultValue false
        test <@ contains @>
      
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |], MaxTest = 200)>]
    let ``Result element belongs to input`` (calendarVersions: CalendarVersion[]) =
        let max = calendarVersions |> tryMax
        let contains = calendarVersions |> Array.contains max.Value
        contains
        
    [<Property>]
    let ``Order invariance (array reversal does not affect result)`` (calendarVersions: CalendarVersion[]) =
        calendarVersions
        |> tryMax = tryMax (Array.randomShuffle calendarVersions)
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |])>]
    let ``Duplicate tolerance`` (calendarVersions: CalendarVersion[]) =
        let max = tryMax calendarVersions
        let duplicated = Array.append calendarVersions [| max.Value |]
        let max' = tryMax duplicated
        test <@ max' = max @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersions> |])>]
    let ``Result is maximum`` (calendarVersions: CalendarVersion[]) =
        let max = calendarVersions |> tryMax                
        calendarVersions
        |> Array.forall (fun v -> compare v max.Value <= 0)
        
module StableTests =        
    [<Fact>]
    let ``Nightly CalendarVersion when release to stable keeps the same Year, Month and Patch when the year and month are the same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
        let monthStamp = { Year = 2023us; Month = 10uy }
        let release = stable calVer monthStamp
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsNone @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to stable changes the Year, Month and removes Patch when the year and month are differed`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
        let monthStamp = { Year = 2023us; Month = 11uy }
        let release = stable calVer monthStamp
        test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
        
    [<Fact>]
    let ``Stable CalendarVersion when release to stable changes Patch, when the year and month are the same`` () =
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }        
        let monthStamp = { Year = 2023us; Month = 10uy }
        let release = stable calVer monthStamp
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch > calVer.Patch && release.Build.IsNone @>        
    
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Stable CalendarVersion releases expected Year`` (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
        let release = stable calVer monthStamp
        release.Year = uint16 monthStamp.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``CalendarVersion stable release Month when the MonthStamp Month is greater``
        (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
        let release = stable calVer monthStamp
        release.Month = byte monthStamp.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
    let ``Three-part CalendarVersion stable release only Patch when the MonthStamp has the same Year and Month`` (calVerWithPatch: CalendarVersion) =
        let monthStamp = { Year = calVerWithPatch.Year; Month = calVerWithPatch.Month }
        let release = stable calVerWithPatch monthStamp
        release.Patch > calVerWithPatch.Patch &&
        release.Month = calVerWithPatch.Month &&
        release.Year = calVerWithPatch.Year
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
    let ``CalendarVersion preserves Year and Month when the MonthStamp has the same Year and Month`` (calVerWithPatch: CalendarVersion)=
        let monthStamp = { Year = calVerWithPatch.Year; Month = calVerWithPatch.Month }
        let release = stable calVerWithPatch monthStamp
        release.Year = calVerWithPatch.Year &&
        release.Month = calVerWithPatch.Month
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionPatch> |])>]
    let ``Three-part CalendarVersion reset Patch when the MonthStamp Year and/or Month is different`` (calVerWithPatch: CalendarVersion, dateTimeOffset: System.DateTimeOffset)=
        let monthStamp = Internals.uniqueMonthStamp (calVerWithPatch, dateTimeOffset)
        let release = stable calVerWithPatch monthStamp
        release.Patch = None
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShort> |])>]
    let ``Two-part CalendarVersion still has None Patch when the MonthStamp Year and/or Month is greater`` (calVer: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let monthStamp = Internals.uniqueMonthStamp (calVer, dateTimeOffset)
        let release = stable calVer monthStamp
        release.Patch = None
        
module NightlyTests =
    [<Fact>]
    let ``Stable CalendarVersion when release to nightly keeps the same Year, Month, increase Patch and adds Build when the year and month are the same`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = None }
        let dayOfMonth = 31uy
        let monthStamp = { Year = 2023us; Month = 10uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch > calVer.Patch && release.Build.IsSome @>
        
    [<Fact>]
    let ``Stable CalendarVersion when release to nightly keeps the same Year, Month, adds Patch and adds Build when the year and month are the same and Patch is none`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = None; Build = None }
        let dayOfMonth = 7uy
        let monthStamp = { Year = 2023us; Month = 10uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch.IsSome && release.Build.IsSome @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to nightly keeps the same Year, Month, the same Patch, the same Day and increases Build's Number when the year, month, patch, day are the same`` () =
        let dayOfMonth = 31uy
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = dayOfMonth; Number = 1us }) }        
        let monthStamp = { Year = 2023us; Month = 10uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to nightly changes day/number and keeps the same Patch when the same Year, Month, Patch and the day is differed`` () =        
        let calVer = { Year = 2023us; Month = 11uy; Patch = Some 4u; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
        let dayOfMonth = 2uy
        let monthStamp = { Year = 2023us; Month = 11uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to nightly adds new Patch when the same Year, Month, None Patch and the day is differed`` () =        
        let calVer = { Year = 2023us; Month = 11uy; Patch = None; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
        let dayOfMonth = 2uy
        let monthStamp = { Year = 2023us; Month = 11uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to nightly keeps the same Year, Month, Day and increases Build's Number when the year, month, day are the same`` () =        
        let calVer = { Year = 2023us; Month = 11uy; Patch = None; Build = Some (Build.Nightly { Day = 1uy; Number = 1us }) }
        let dayOfMonth = 1uy
        let monthStamp = { Year = 2023us; Month = 11uy }
        let release = nightly calVer (dayOfMonth, monthStamp)
        test <@ release.Year = calVer.Year && release.Month = calVer.Month && release.Patch = calVer.Patch && release.Build.IsSome @>
        
    [<Fact>]
    let ``Nightly CalendarVersion when release to stable change the Year, Month and removes Patch when the year and month are differed`` () =        
        let calVer = { Year = 2023us; Month = 10uy; Patch = Some 1u; Build = Some (Build.Nightly { Day = 31uy; Number = 1us }) }
        let monthStamp = { Year = 2023us; Month = 11uy }
        let release = stable calVer monthStamp
        test <@ release.Year = monthStamp.Year && release.Month = monthStamp.Month && release.Patch.IsNone && release.Build.IsNone @>
        
    //Year Rollover
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``The calendar version Nightly returns version with the MonthStamp Year when the MonthStamp Year is different from the calendar version Year``
        (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = byte dateTimeOffset.Day
        let monthStamp =
         if uint16 dateTimeOffset.Year <> calendarVersion.Year
          then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
          else { Year = uint16 (dateTimeOffset.AddYears(1).Year); Month = byte dateTimeOffset.Month }          
        let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
        test <@ nightly.Year = monthStamp.Year @>
        
    // Month Rollover
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``The calendar version Nightly returns version with the MonthStamp Month when the MonthStamp Month is different from the calendar version Month``
        (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = byte dateTimeOffset.Day
        let monthStamp =
         if byte dateTimeOffset.Month <> calendarVersion.Month
          then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
          else { Year = uint16 dateTimeOffset.Year; Month = byte (dateTimeOffset.AddMonths(1).Month) }
        let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
        test <@ nightly.Month = monthStamp.Month @>
        
    // Build is always updated
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``The calendar version Nightly always updates its build``
        (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = byte dateTimeOffset.Day
        let monthStamp = { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
        let nightly = nightly calendarVersion (dayOfMonth, monthStamp)        
        test <@ calendarVersion.Build <> nightly.Build @>    
    
    // Build Day is correct
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``The calendar version Nightly returns a build with the correct day of month``
        (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        // prepare data
        let dayOfMonth =
            if calendarVersion.Build
               |> Option.map (function | Build.Nightly { Day = day } -> day = byte dateTimeOffset.Day)
               |> Option.defaultValue false
            then byte (dateTimeOffset.AddDays(1).Day)
            else byte dateTimeOffset.Day
        let monthStamp = { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
        let nightly = nightly calendarVersion (dayOfMonth, monthStamp)
        test <@ nightly.Build |> Option.map (function | Build.Nightly { Day = day } -> day = dayOfMonth) |> Option.defaultValue false @>    
    
    // Patch is reset
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``The calendar version Nightly resets Patch when the MonthStamp Year and/or Month is different from the calendar version``
        (calendarVersion: CalendarVersion, dateTimeOffset: System.DateTimeOffset) =
        let dayOfMonth = byte dateTimeOffset.Day
        let monthStamp =
         if uint16 dateTimeOffset.Year <> calendarVersion.Year ||
            byte dateTimeOffset.Month <> calendarVersion.Month            
          then { Year = uint16 dateTimeOffset.Year; Month = byte dateTimeOffset.Month }
          else { Year = uint16 (dateTimeOffset.AddYears(1).Year); Month = byte (dateTimeOffset.AddMonths(1).Month) }
        let nightly = nightly calendarVersion (dayOfMonth, monthStamp)
        test <@ nightly.Patch.IsNone @>
        
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
      
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersionShortNightlyBuild> |])>]
    let ``CalendarVersion with Build contains it's Build in the string representation`` (calendarVersion: CalendarVersion) =
        let calVerString = calendarVersion |> toString
        let contains = match calendarVersion with
                        | { Build = Some (Build.Nightly { Day = day; Number = number }) } ->
                            calVerString.Contains($"{day}") &&
                            calVerString.Contains($"{number}")
                        | _ -> failwith "CalendarVersion with Build should have a Build"
        test <@ contains @>
        
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
        
    // Commit string contains 2 whitespace
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.calendarVersion> |])>]
    let ``Commit message contains two whitespaces on its format`` (calendarVersion: CalendarVersion) =
        let commitMsg = toCommitMessage calendarVersion
        let spaceCount = commitMsg |> Seq.filter (fun c -> c = ' ') |> Seq.length
        test <@ spaceCount = 2 @>