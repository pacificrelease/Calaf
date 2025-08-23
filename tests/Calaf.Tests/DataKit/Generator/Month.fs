module Calaf.Tests.Generator.Month

open FsCheck.FSharp

open Calaf.Tests.Generator.Primitive

let inRangeByteMonth =
    gen {
        let! month = Gen.choose(int Calaf.Domain.Month.LowerMonthBoundary,
                                int Calaf.Domain.Month.UpperMonthBoundary)
        return byte month
    }
    
let outOfRangeByteMonth =            
    gen {
        let! month = Gen.choose(int Calaf.Domain.Month.UpperMonthBoundary + 1, int System.Byte.MaxValue)
        return month 
    }
    
let leadingZeroOutOfRangeStringMonth =
    gen {
        let! month = outOfRangeByteMonth
        return $"{month:D6}"
    }
    
let wrongInt32Month =
    let wrongCornerCases = Gen.elements [ -1;                         
                                          System.Int32.MinValue
                                          System.Int32.MaxValue
                                          int System.Byte.MaxValue + 1]
    let smallWrongInt32 = Gen.choose(int System.Int32.MinValue, int System.Byte.MinValue - 1)
    let bigWrongInt32   = Gen.choose(int System.Byte.MaxValue + 1, System.Int32.MaxValue)
    gen {
        return! Gen.frequency [
            1, wrongCornerCases
            2, smallWrongInt32
            2, bigWrongInt32
        ]
    }
    
let wrongStringMonth =
    let genTooBig = Gen.choose64(int64 (System.Byte.MaxValue + 1uy), System.Int64.MaxValue)                
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