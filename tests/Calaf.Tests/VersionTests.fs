module Calaf.Tests.VersionTests

open FsCheck.Xunit

open Calaf.Domain.DomainTypes
open Calaf.Domain.Version

// Done
// Three-part CalVer format
// Strings with format {year}.{month}.{patch} parse to CalVer with those values

// Done
// Two-part CalVer format
// Strings with format {year}.{month} parse to CalVer with those values and no patch

// Done
// SemVer-like format
// Strings matching SemVer pattern but not CalVer return LooksLikeSemVer

// Done
// Invalid format
// Strings with invalid format return Unsupported

// Invalid parts
// Valid format but invalid year/month/patch values return correct version type

// Empty string
// Empty string returns Unsupported

module TryParsePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validThreePartCalVerString> |], MaxTest = 200)>]
    let ``Valid three-part CalVer string parses to their corresponding values`` (validVersion: string) =
        let parts = validVersion.Split('.')
        let expectedYear = uint16 parts.[0]
        let expectedMonth = byte parts.[1]
        let expectedPatch = uint32 parts.[2]

        match tryParse validVersion with
        | Some (CalVer calVer) ->
            calVer.Year = expectedYear &&
            calVer.Month = expectedMonth &&
            calVer.Patch = Some expectedPatch
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validTwoPartCalVerString> |], MaxTest = 200)>]
    let ``Valid two-part CalVer string parses to their corresponding values`` (validVersion: string) =
        let parts = validVersion.Split('.')
        let expectedYear = uint16 parts.[0]
        let expectedMonth = byte parts.[1]

        match tryParse validVersion with
        | Some (CalVer calVer) ->
            calVer.Year = expectedYear &&
            calVer.Month = expectedMonth &&
            calVer.Patch = None
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.validSemVerString> |], MaxTest = 200)>]
    let ``Valid SemVer string parses to LooksLikeSemVer`` (semVerVersion: string) =
        let parts = semVerVersion.Split('.')
        let expectedMajor = uint32 parts.[0]
        let expectedMinor = uint32 parts.[1]
        let expectedPatch = uint32 parts.[2]

        match tryParse semVerVersion with
        | Some (LooksLikeSemVer semVer) ->
            semVer.Major = expectedMajor &&
            semVer.Minor = expectedMinor &&
            semVer.Patch = expectedPatch
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.nonNumericString> |], MaxTest = 200)>]
    let ``Invalid string parses to Unsupported`` (invalidVersion: string) =
        invalidVersion
        |> tryParse = Some Unsupported