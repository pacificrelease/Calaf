namespace Calaf.Tests.Generator

open FsCheck.FSharp

module Day =
    let inRangeByteDay =
        gen {
            let! day = Gen.choose(int Calaf.Domain.Day.LowerDayBoundary,
                                  int Calaf.Domain.Day.UpperDayBoundary - 3)
            return byte day
        }
        
    let outOfRangeByteDay =            
        gen {
            let! day = Gen.choose(int Calaf.Domain.Day.UpperDayBoundary + 1, int System.Byte.MaxValue)
            return day 
        }
        
    let wrongInt32Day =
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