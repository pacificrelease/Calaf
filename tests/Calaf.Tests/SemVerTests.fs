namespace Calaf.Tests.SemVerTests

open FsCheck.Xunit
open Xunit

open Calaf.Domain.Patch
open Calaf.Tests

// Valid numeric strings
// Valid UInt32 strings should parse and convert to the correct type

// Invalid inputs
// Non-numeric strings, negative numbers, overflows return None

// Boundary values
// Zero, max UInt32 value, and edge cases parse correctly

// Format variations
// Leading zeros, whitespace trimming handled according to UInt32.TryParse rules

// Type conversion correctness
// Explicit operator correctly converts from UInt32 to target type

// Culture invariance
// Parsing works correctly regardless of current culture

module TryParseFromStringPropertyTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.validPatchUInt32> |], MaxTest = 200)>]
    let ``Valid string parses to the Some + corresponding value`` (validPatch: uint32) =
        validPatch
        |> string
        |> tryParseFromString  = Some validPatch