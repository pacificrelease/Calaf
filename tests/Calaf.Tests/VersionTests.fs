namespace Calaf.Tests.VersionTests

open FsCheck.Xunit

open Calaf.Domain.DomainTypes
open Calaf.Domain.Version
open Calaf.Tests

module TryParsePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validThreePartCalVerString> |], MaxTest = 200)>]
    let ``Valid three-part CalVer string parses to their corresponding values`` (validVersion: string) =
        let parts = validVersion.Split('.')
        let expectedYear = uint16 parts[0]
        let expectedMonth = byte parts[1]
        let expectedPatch = uint32 parts[2]

        match tryParse validVersion with
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

        match tryParse validVersion with
        | Some (CalVer calVer) ->
            calVer.Year = expectedYear &&
            calVer.Month = expectedMonth &&
            calVer.Patch = None
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validSemVerString> |], MaxTest = 200)>]
    let ``Valid SemVer string parses to LooksLikeSemVer`` (semVerVersion: string) =
        let parts = semVerVersion.Split('.')
        let expectedMajor = uint32 parts[0]
        let expectedMinor = uint32 parts[1]
        let expectedPatch = uint32 parts[2]

        match tryParse semVerVersion with
        | Some (SemVer semVer) ->
            semVer.Major = expectedMajor &&
            semVer.Minor = expectedMinor &&
            semVer.Patch = expectedPatch
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to Unsupported`` (invalidVersion: string) =
        invalidVersion
        |> tryParse = Some Unsupported
        
    [<Property(Arbitrary = [| typeof<Arbitrary.invalidThreePartString> |], MaxTest = 200)>]
    let ``Invalid CalVer/SemVer but valid three-part format parses to Unsupported`` (invalidVersion: string) =
        invalidVersion
        |> tryParse = Some Unsupported
        
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