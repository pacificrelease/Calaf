namespace Calaf.Tests

open FsCheck.FSharp

module Generator =
    let private genNegative =
        gen {
            let! neg = Gen.choose(-99999, -1)
            return string neg
        }
        
    let private genFloat =
        gen {
            let! whole = Gen.choose(0, 10000)
            let! frac = Gen.choose(1, 999999)
            return $"{whole}.{frac:D6}"
        }
        
    let greaterThanZeroBeforeUInt32MinusOne =
        gen {
            let! genLessThanMax = Gen.choose64(1L, (int64 System.UInt32.MaxValue - 1L))
            return uint32 genLessThanMax
        }
        
    let nonNumericString =
        let letterChars = ['a'..'z'] @ ['A'..'Z']
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        let letterOrSpecialChars = letterChars @ specialChars

        let genStringFromChars chars =
            gen {
                let! length = Gen.choose(1, 20)
                let! charArray = Gen.arrayOfLength length (Gen.elements chars)
                return System.String(charArray)
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
                return string big
            }
        
        gen {
            let! choice = Gen.frequency [
                3, genTooBig
                2, genNegative
                1, genFloat
            ]
            return choice
        }
        
    let validMonthByte =
        gen {
            let! month = Gen.choose(1, 12)
            return byte month
        }
        
    let leadingZeroDigitString =
        gen {
            let! month = Gen.choose(1, 9)
            return $"0{month}" 
        }
        
    let overflowMonthString =
        let genTooBig =
            gen {
                let! overflow = Gen.choose64(13L, System.Int64.MaxValue)
                return string overflow
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
    type greaterThanZeroBeforeUInt32MinusOne =
        static member greaterThanZeroUInt32() =
            Arb.fromGen Generator.greaterThanZeroBeforeUInt32MinusOne
            
    type internal nonNumericString =
        static member nonNumericString() =
            Arb.fromGen Generator.nonNumericString
            
    type internal overflowUInt32String =
        static member overflowUInt32String() =
            Arb.fromGen Generator.overflowUInt32String
            
    type internal validMonthByte =
        static member validMonthByte() =
            Arb.fromGen Generator.validMonthByte
            
    type internal leadingZeroDigitString =
        static member leadingZeroDigitString() =
            Arb.fromGen Generator.leadingZeroDigitString
            
    type internal overflowMonthString =
        static member overflowMonthString() =
            Arb.fromGen Generator.overflowMonthString