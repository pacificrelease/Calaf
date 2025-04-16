namespace Calaf.Tests

open FsCheck.FSharp

module Generator =
    
    
    
    let greaterThanZeroUInt32 =
        gen {
            let! hi = Gen.choose(0, 0xFFFF)
            let! lo = Gen.choose(0, 0xFFFF)
            let combined = (uint32 hi <<< 16) ||| uint32 lo
            if combined = 0u then
                return 1u
            else
                return combined
        }
        
    let nonNumericString =
        let letterChars = ['a'..'z'] @ ['A'..'Z']
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        let letterOrSpecialChars = letterChars @ specialChars

        let genStringFromChars chars =
            gen {
                let! length = Gen.choose(1, 20)
                let! charArray = Gen.arrayOfLength length (Gen.elements chars)
                return string(charArray)
            }
            
        let genGarbage =
            gen {
                let! garbage = Gen.elements ["NaN"; "Infinity"; "-Infinity"]
                return garbage
            }
            
        gen {
            let! choice = Gen.frequency [
                4, genStringFromChars letterChars
                3, genStringFromChars letterOrSpecialChars      
                1, genGarbage
            ]
            return choice
        }
        
    let overflowUInt32String =
        let genTooBig =
            gen {        
                let! big = Gen.choose64(int64 System.UInt32.MaxValue + 1L, System.Int64.MaxValue)
                return big |> string
            }

        let genNegative =
            gen {
                let! neg = Gen.choose(-99999, -1)
                return neg |> string
            }

        let genFloat =
            gen {
                let! whole = Gen.choose(0, 10000)
                let! frac = Gen.choose(1, 999999)
                return $"{whole}.{frac:D6}"
            }        
        
        gen {
            let! choice = Gen.frequency [
                3, genTooBig
                2, genNegative
                1, genFloat
            ]
            return choice
        }

module Arbitrary =
    type greaterThanZeroUInt32 =
        static member greaterThanZeroUInt32() =
            Arb.fromGen Generator.greaterThanZeroUInt32
            
    type internal nonNumericString =
        static member nonNumericString() =
            Arb.fromGen Generator.nonNumericString
            
    type internal overflowUInt32String =
        static member overflowUInt32String() =
            Arb.fromGen Generator.overflowUInt32String