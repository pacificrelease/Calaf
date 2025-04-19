namespace Calaf.Tests

open FsCheck.FSharp

open Calaf.Domain.DomainTypes

type TimeStampIncrement = Year | Month | Both

module Generator =
    let private genNegative =
        gen {
            let! neg = Gen.choose(System.Int32.MinValue, -1)
            return neg
        }
        
    let private genFloat =        
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
        
    let validPatchUInt32 =
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
                let! garbage = Gen.elements ["NaN"; "Infinity"; "-Infinity"; ""; " "]
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
        
    let overflowPatchString =
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
        
    let leadingZeroUInt16String =
        gen {
            let! year = Gen.choose(1, System.UInt16.MaxValue |> int)
            return $"{year:D6}"
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
                2, genNegative |> Gen.map string
                1, genFloat
            ]
            return choice
        }
        
    let overflowMonthInt32 =
        let genLittleBig =
            Gen.elements [0; 13; -1; System.Int32.MinValue; System.Int32.MaxValue]
            
        let genTooBig =
            Gen.choose(13, System.Int32.MaxValue)
            
        gen {
            let! choice = Gen.frequency [
                1, genLittleBig
                3, genTooBig
                3, genNegative
            ]
            return choice
        }
        
    let validYearUInt16 =
        gen {
            let! year = Gen.choose(1, System.UInt16.MaxValue |> int)
            return uint16 year
        }
        
    let overflowYearCornerCases =
            Gen.elements [0; -1; int System.UInt16.MinValue; int System.UInt16.MaxValue + 1]
        
    let overflowYearString =
        let genTooBig =
            gen {
                let! overflow = Gen.choose64(int64 (System.UInt16.MaxValue + 1us), System.Int64.MaxValue)
                return string overflow
            }
            
        gen {
            let! choice = Gen.frequency [
                2, genTooBig
                2, genNegative |> Gen.map string
                1, overflowYearCornerCases |> Gen.map string
                1, genFloat
            ]
            return choice
        }
        
    let overflowYearInt32 =
        let genLittleBig =
            Gen.elements [0; -1; System.Int32.MinValue; int System.UInt16.MaxValue + 1]
            
        let genTooBig =
            Gen.choose(int System.UInt16.MaxValue + 1, System.Int32.MaxValue)
            
        gen {
            let! choice = Gen.frequency [
                1, genLittleBig
                3, genTooBig
                3, genNegative
            ]
            return choice
        }
        
    let validThreePartCalVerString =
        gen {
            let! year = validYearUInt16
            let! month = validMonthByte
            let! patch = validPatchUInt32
            return $"{year}.{month}.{patch}"
        }
        
    let validTwoPartCalVerString =
        gen {
            let! year = validYearUInt16
            let! month = validMonthByte
            return $"{year}.{month}"
        }
        
    let validSemVerString =
        let genBigSemVer =
            gen {
                let! major = Gen.choose64(0, int64 System.UInt32.MaxValue)
                let! minor = Gen.choose64(0, int64 System.UInt32.MaxValue)
                let! patch = Gen.choose64(0, int64 System.UInt32.MaxValue)
                return $"{major}.{minor}.{patch}"
            }
        let genSmallSemVer =
            gen {
                let! major = Gen.choose64(0, 999)
                let! minor = Gen.choose64(0, 99999)
                let! patch = Gen.choose64(1, 99999)
                return $"{major}.{minor}.{patch}"
            }
        gen {
            let! choice = Gen.frequency [
                1, genBigSemVer
                1, genSmallSemVer
            ]
            return choice
        }
        
    let invalidThreePartString =
        gen {
            let! first  = nonNumericString
            let! second = nonNumericString
            let! third  = nonNumericString
            return $"{first}.{second}.{third}"
        }
    
    let twoSectionCalendarVersion =
        gen {
            let! year = validYearUInt16
            let! month = validMonthByte
            return { Year = year; Month = month; Patch = None }
        }
        
    let threeSectionCalendarVersion =
        gen {
            let! calVer = twoSectionCalendarVersion
            let! patch = validPatchUInt32
            return { calVer with Patch = Some patch }
        }    
        
    let calendarVersion =
        gen {
            let! threeSectionCalVer = Gen.elements [true; false]
            return! if threeSectionCalVer
                then threeSectionCalendarVersion
                else twoSectionCalendarVersion
        }
        
    let calendarVersions =
        gen {
            let! smallCount = Gen.choose(1, 50)
            let! middleCount = Gen.choose(51, 1000)            
            let! bigCount = Gen.choose(1001, 25_000)            
            let! choice = Gen.frequency [
                3, Gen.arrayOfLength smallCount calendarVersion
                2, Gen.arrayOfLength middleCount calendarVersion
                1, Gen.arrayOfLength bigCount calendarVersion
            ]
            return choice
        }
        
    let private timeStampYearUpperBoundary = System.DateTime.MaxValue.Year - 1
    
    let twoPartCalendarVersionWithSameTimeStamp =
        let genCalVer =
            gen { 
                let! year  = Gen.choose(1, timeStampYearUpperBoundary)
                let! month = Gen.choose(1, 11)       
                return { Year = uint16 year; Month = byte month; Patch = None }
            }            
        gen {
            let! calVer = genCalVer
            let! day = Gen.choose(1, 28)
            let timeStamp = (int calVer.Year, int calVer.Month, day) |> System.DateTime
            return (calVer, timeStamp)
        }
        
    let threePartCalendarVersionWithSameTimeStamp =            
        gen {
            let! calVer, timeStamp = twoPartCalendarVersionWithSameTimeStamp
            let! patch = validPatchUInt32 
            return ({ calVer with Patch = Some patch }, timeStamp)
        }
        
    let calendarVersionWithSameTimeStamp =
        gen {
            let! threeSectionCalVer = Gen.elements [true; false]
            return! if threeSectionCalVer
                then threePartCalendarVersionWithSameTimeStamp
                else twoPartCalendarVersionWithSameTimeStamp
                        
        }
        
    let timeStampIncrement =
        gen {
            return! Gen.elements [ TimeStampIncrement.Year; TimeStampIncrement.Month; TimeStampIncrement.Both ]
        }

module Arbitrary =
    type internal validPatchUInt32 =
        static member validPatchUInt32() =
            Arb.fromGen Generator.validPatchUInt32
            
    type internal greaterThanZeroBeforeUInt32MinusOne =
        static member greaterThanZeroUInt32() =
            Arb.fromGen Generator.greaterThanZeroBeforeUInt32MinusOne
            
    type internal nonNumericString =
        static member nonNumericString() =
            Arb.fromGen Generator.nonNumericString
            
    type internal overflowPatchString =
        static member overflowPatchString() =
            Arb.fromGen Generator.overflowPatchString
            
    type internal validMonthByte =
        static member validMonthByte() =
            Arb.fromGen Generator.validMonthByte
            
    type internal leadingZeroDigitString =
        static member leadingZeroDigitString() =
            Arb.fromGen Generator.leadingZeroDigitString
            
    type internal overflowMonthString =
        static member overflowMonthString() =
            Arb.fromGen Generator.overflowMonthString
            
    type internal overflowMonthInt32 =
        static member overflowMonthInt32() =
            Arb.fromGen Generator.overflowMonthInt32
            
    type internal validYearUInt16 =
        static member validYearUInt16() =
            Arb.fromGen Generator.validYearUInt16
            
    type internal leadingZeroUInt16String =
        static member leadingZeroUInt16String() =
            Arb.fromGen Generator.leadingZeroUInt16String
            
    type internal overflowYearString =
        static member overflowYearString() =
            Arb.fromGen Generator.overflowYearString
            
    type internal overflowYearInt32 =
        static member overflowYearInt32() =
            Arb.fromGen Generator.overflowYearInt32
            
    type internal validThreePartCalVerString =
        static member validThreePartCalVerString() =
            Arb.fromGen Generator.validThreePartCalVerString
            
    type internal validTwoPartCalVerString =
        static member validTwoPartCalVerString() =
            Arb.fromGen Generator.validTwoPartCalVerString
            
    type internal validSemVerString =
        static member validSemVerString() =
            Arb.fromGen Generator.validSemVerString
            
    type internal invalidThreePartString =
        static member invalidThreePartString() =
            Arb.fromGen Generator.invalidThreePartString
            
    type internal calendarVersion =
        static member calendarVersion() =
            Arb.fromGen Generator.calendarVersion

    type internal twoSectionCalendarVersion =
        static member twoSectionCalendarVersion() =
            Arb.fromGen Generator.twoSectionCalendarVersion
            
    type internal threeSectionCalendarVersion =
        static member threeSectionCalendarVersion() =
            Arb.fromGen Generator.threeSectionCalendarVersion            
            
    type internal calendarVersions =
        static member calendarVersions() =
            Arb.fromGen Generator.calendarVersions
    
    type internal twoPartCalendarVersionWithSameTimeStamp =
        static member twoPartCalendarVersionWithSameTimeStamp() =
            Arb.fromGen Generator.twoPartCalendarVersionWithSameTimeStamp
            
    type internal threePartCalendarVersionWithSameTimeStamp =
        static member threePartCalendarVersionWithSameTimeStamp() =
            Arb.fromGen Generator.threePartCalendarVersionWithSameTimeStamp
            
    type internal calendarVersionWithSameTimeStamp =
        static member calendarVersionWithSameTimeStamp() =
            Arb.fromGen Generator.calendarVersionWithSameTimeStamp
            
    type internal timeStampIncrement =
        static member timeStampIncrement() =
            Arb.fromGen Generator.timeStampIncrement