module Calaf.Tests.Generator.Primitive

open FsCheck.FSharp

let genBool =
    Gen.elements [true; false]
    
let genNegative =
    gen {
        let! neg = Gen.choose(System.Int32.MinValue, -1)
        return neg
    }
    
let genFloat = 
    gen {
        let! sign = Gen.elements [""; "-"]
        let! integer = Gen.choose(0, 1_000_000)
        let! fraction = Gen.choose(1, 1_000_000)
        return $"{sign}{integer}.{fraction:D6}"
    }
    
let greaterThanZeroBeforeUInt32MinusOne =
    gen {
        let! genLessThanMax = Gen.choose64(1L, (int64 System.UInt32.MaxValue - 1L))
        return uint32 genLessThanMax
    }
    
let genTagVersionPrefix =
    Gen.elements Calaf.Domain.Version.versionPrefixes
    
let genReleaseCycle =
    gen {
        let! tag = Gen.elements ["pre-alpha"; "alpha"; "beta"; "rc"; "rtm"; "ga"; "production"; "stable"]
        return tag
    }
    
let genWhiteSpacesString =
    let whitespaceChars = [' '; '\t']
    let shortLength = 20
    let longLength = int System.Byte.MaxValue

    let genRandomWhitespace minLen maxLen =
        gen {
            let! length = Gen.choose(minLen, maxLen)
            let! chars = Gen.arrayOfLength length (Gen.elements whitespaceChars)
            return System.String(chars)
        }
    
    let genSpacesOnly minLen maxLen =
        gen {
            let! length = Gen.choose(minLen, maxLen)
            let! chars = Gen.arrayOfLength length (Gen.constant ' ')
            return System.String(chars)
        }

    gen {
        return! Gen.frequency [
            2, genRandomWhitespace 1 shortLength
            2, genSpacesOnly 1 shortLength
            1, genRandomWhitespace (shortLength + 1) longLength
            1, genSpacesOnly (shortLength + 1) longLength
        ]
    }
    
let genValidDateTimeOffset =
    gen {
        let min   = System.DateTimeOffset(System.DateTime(int Calaf.Domain.Year.LowerYearBoundary, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))
        let max   = System.DateTimeOffset(System.DateTime(int Calaf.Domain.Year.UpperYearBoundary, 12, 31, 23, 59, 59, 999, System.DateTimeKind.Utc))
        let daysMax = int (max - min).TotalDays
        let! days     = Gen.choose (0, daysMax)
        let! seconds  = Gen.choose (0, 86_399)
        let! millis   = Gen.choose (0, 999)
        let utcInstant = min.AddDays(float days).AddSeconds(float seconds).AddMilliseconds(float millis)
        let! offsetMinutes = Gen.elements [ for m in -14*60 .. 30 .. 14*60 -> m ]
        let offset = System.TimeSpan.FromMinutes(float offsetMinutes)
        return utcInstant.ToOffset(offset)
    }
    
let genCommitHash =     
    Gen.constant <| Bogus.Faker().Random.Hash()
    
let genHexadecimal =
    Gen.constant <| Bogus.Faker().Random.Hexadecimal()
    
let genFrom1To512LettersString =
    Gen.constant <| Bogus.Faker().Random.String2(1, 512)
    
let genUInt16 =
    Gen.choose(1, int System.UInt16.MaxValue) |> Gen.map uint16
    
let genDay =
    gen {
        let! day = Gen.choose(1, 31)
        return byte day
    }
    
let validMicroUInt32 =
    Gen.choose64(1L, int64 System.UInt32.MaxValue) |> Gen.map uint32    
    
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
            let! garbage = Gen.elements ["NaN"; "Infinity"; "-Infinity"; "null"; "undefined"; "true"; "false"]
            return garbage
        }
        
    let onlyDots =
        gen {
            let! length = Gen.choose(1, 20)
            let! dots = Gen.arrayOfLength length (Gen.constant '.')
            return dots |> System.String
        }
        
    gen {
        let! choice = Gen.frequency [
            4, genStringFromChars letterChars
            3, genStringFromChars letterOrSpecialChars
            2, onlyDots
            1, genGarbage
        ]
        return choice
    }

let nullOrWhiteSpaceString =
    gen {
        return! Gen.elements [""; " "; null]
    }
    
let overflowMicroString =
    let genZero =
        Gen.constant "0"
        
    let genTooBig =
        gen {        
            let! big = Gen.choose64(int64 System.UInt32.MaxValue + 1L, System.Int64.MaxValue)
            return string big
        }        
    
    gen {
        let! choice = Gen.frequency [
            3, genTooBig
            2, genNegative |> Gen.map string                
            2, genFloat
            1, genZero
        ]
        return choice
    }
    
let directoryPathString =
    Gen.constant (Bogus.Faker().System.DirectoryPath())
    
let invalidThreePartString =
    gen {
        let! first  = nonNumericString
        let! second = nonNumericString
        let! third  = nonNumericString
        return $"{first}.{second}.{third}"
    }