module Calaf.Tests.Generator.Year

open FsCheck.FSharp

open Calaf.Tests.Generator.Primitive

let inRangeUInt16Year =
    gen {
        let! year =
            Gen.choose(int Calaf.Domain.Year.LowerYearBoundary,
                       int Calaf.Domain.Year.UpperYearBoundary)
        return uint16 year
    }
    
let outOfRangeUInt16Year =
    let outOfRangeCornerCases = Gen.elements [ 0;
                                               int Calaf.Domain.Year.LowerYearBoundary - 1
                                               int Calaf.Domain.Year.UpperYearBoundary + 1 ]
    let outOfRangeLowerThaAllowed = Gen.choose(int System.UInt16.MinValue, int Calaf.Domain.Year.LowerYearBoundary - 1)
    let outOfRangeGreaterThaAllowed = Gen.choose(int Calaf.Domain.Year.UpperYearBoundary + 1, int System.UInt16.MaxValue)
    gen {
        let! year = Gen.frequency [
            1, outOfRangeCornerCases
            1, outOfRangeLowerThaAllowed
            1, outOfRangeGreaterThaAllowed
        ]
        return uint16 year 
    }
    
let leadingZeroOutOfRangeStringYear =
    gen {
        let! lowerThanAllowed   = Gen.constant 0
        let! greaterThanAllowed = Gen.choose(int Calaf.Domain.Year.UpperYearBoundary + 1, int System.UInt16.MaxValue)
        let! year = Gen.frequency [
            1, Gen.constant lowerThanAllowed
            3, Gen.constant greaterThanAllowed
        ]
        return $"{year:D6}"
    }
    
let wrongInt32Year =
    let wrongCornerCases = Gen.elements [ -1;                         
                                          System.Int32.MinValue
                                          System.Int32.MaxValue
                                          int System.UInt16.MaxValue + 1]
    let smallWrongInt32 = Gen.choose(int System.Int32.MinValue, int System.UInt16.MinValue - 1)
    let bigWrongInt32   = Gen.choose(int System.UInt16.MaxValue + 1, System.Int32.MaxValue)
    gen {
        return! Gen.frequency [
            1, wrongCornerCases
            2, smallWrongInt32
            2, bigWrongInt32
        ]
    }
    
let wrongStringYear =
    let genTooBig = Gen.choose64(int64 (System.UInt16.MaxValue + 1us), System.Int64.MaxValue)
        
    gen {
        let! choice = Gen.frequency [
            3, genTooBig   |> Gen.map string
            3, genNegative |> Gen.map string
            1, nullOrWhiteSpaceString
            1, nonNumericString
            1, genFloat
        ]
        return choice
    }