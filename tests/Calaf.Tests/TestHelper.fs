namespace Calaf.Tests

open FsCheck.FSharp

module Generator =         
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
            
        gen {
            let! choice = Gen.frequency [
                4, genStringFromChars letterChars             // alphabetic strings
                3, genStringFromChars letterOrSpecialChars    // alphabetic + special
                1, gen {
                    let! intPart = Gen.choose(-1000, 1000)
                    let! fracPart = Gen.choose(0, 999999)
                    return $"{intPart}.{fracPart:D6}"
                }
            ]
            return choice
        }

module Arbitrary =            
    type internal nonNumericString =
        static member NonNumericString() =
            Arb.fromGen Generator.nonNumericString