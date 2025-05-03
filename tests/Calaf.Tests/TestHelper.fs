namespace Calaf.Tests

open FsCheck
open FsCheck.FSharp

open Calaf.Contracts
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
        
    let genTagVersionPrefix =
        gen {
            let! prefix = Gen.elements Calaf.Domain.Version.versionPrefixes
            return prefix
        }
        
    let genReleaseCycle =
        gen {
            let! tag = Gen.elements ["pre-alpha"; "alpha"; "beta"; "rc"; "rtm"; "ga"; "production"; "stable"]
            return tag
        }
        
    let genWhiteSpacesString =
        let genMixed l h =
            gen {
                let chars = [' '; '\t'; '\n'; '\r']
                let! length = Gen.choose(l, h)
                return! Gen.arrayOfLength length (Gen.elements chars)
            }
        let genWhiteSpaceOnly l h =
            gen {
                let! length = Gen.choose(l, h)
                return! Gen.arrayOfLength length (Gen.constant ' ')
            }
        gen {
            let! choice = Gen.frequency [
                1, genMixed 1 20 |> Gen.map System.String
                1, genWhiteSpaceOnly 1 20 |> Gen.map System.String
                1, genMixed 21 (int System.Byte.MaxValue) |> Gen.map System.String
                1, genWhiteSpaceOnly 21 (int System.Byte.MaxValue) |> Gen.map System.String
            ]
            return choice
        }
        
    let genValidDateTimeOffset =
        gen {
            let min = System.DateTimeOffset(System.DateTime(int Calaf.Domain.Year.lowerYearBoundary, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))
            let max   = System.DateTimeOffset(System.DateTime(int Calaf.Domain.Year.upperYearBoundary, 12, 31, 23, 59, 59, 999, System.DateTimeKind.Utc))
            let daysMax = int (max - min).TotalDays
            let! days     = Gen.choose (0, daysMax)
            let! seconds  = Gen.choose (0, 86_399)
            let! millis   = Gen.choose (0, 999)
            let utcInstant = min
                                 .AddDays(float days)
                                 .AddSeconds(float seconds)
                                 .AddMilliseconds(float millis)
            let! offsetMinutes = Gen.elements [ for m in -14*60 .. 30 .. 14*60 -> m ]
            let offset = System.TimeSpan.FromMinutes(float offsetMinutes)
            return utcInstant.ToOffset(offset)
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
        
    let leadingZeroNonYearUInt16String =
        gen {
            let! lowerThanAllowed   = Gen.choose(1, int Calaf.Domain.Year.lowerYearBoundary - 1)
            let! greaterThanAllowed = Gen.choose(int Calaf.Domain.Year.upperYearBoundary + 1, int System.UInt16.MaxValue)
            let! year = Gen.frequency [
                1, Gen.constant lowerThanAllowed
                1, Gen.constant greaterThanAllowed
            ]
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
            let! year = Gen.choose(int Calaf.Domain.Year.lowerYearBoundary, int Calaf.Domain.Year.upperYearBoundary)
            return uint16 year
        }
        
    let overflowYearCornerCases =
            Gen.elements [0; -1
                          int Calaf.Domain.Year.lowerYearBoundary - 1
                          int Calaf.Domain.Year.upperYearBoundary + 1
                          int System.UInt16.MinValue
                          int System.UInt16.MaxValue + 1]
        
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
            Gen.elements [0; -1
                          int Calaf.Domain.Year.lowerYearBoundary - 1
                          int Calaf.Domain.Year.upperYearBoundary + 1
                          System.Int32.MinValue
                          int System.UInt16.MaxValue + 1]
            
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
            let! year   = validYearUInt16
            let! month   = validMonthByte
            let! patch = validPatchUInt32
            return $"{year}.{month}.{patch}"
        }    
        
    let validTwoPartCalVerString =
        gen {
            let! year = validYearUInt16
            let! month = validMonthByte
            return $"{year}.{month}"
        }
        
    let validCalVerString =
        gen {            
            let! choice = Gen.frequency [
                1, validThreePartCalVerString
                1, validTwoPartCalVerString
            ]
            return choice
        }
        
    let validSemVerString =
        let genBigSemVer =
            gen {
                let! major = Gen.choose64(1000, int64 System.UInt32.MaxValue)
                let! minor = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                let! patch = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                return $"{major}.{minor}.{patch}"
            }
        let genSmallSemVer =
            gen {
                let! major = Gen.choose(0, 999)
                let! minor = Gen.choose(13, 99999)
                let! patch = Gen.choose(1, 99999)
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
        
    let validTagCalVerString =
        gen {
            let! validPrefix = genTagVersionPrefix
            let! choice = Gen.frequency [
                1, validThreePartCalVerString
                1, validTwoPartCalVerString
            ]
            return $"{validPrefix}{choice}"
        }
        
    let validTwoPartTagCalVerString =
        gen {
            let! prefix = genTagVersionPrefix
            let! version = validTwoPartCalVerString
            return $"{prefix}{version}"
        }
        
    let validTagSemVerString =
        gen {
            let! validPrefix = genTagVersionPrefix
            let! semVer = validSemVerString
            return $"{validPrefix}{semVer}"
        }
        
    let whiteSpaceLeadingTrailingValidCalVerString =
        gen {
            let! validCalVerString = validCalVerString
            
            let! whiteSpacesPrefix = genWhiteSpacesString
            let! whiteSpacesSuffix = genWhiteSpacesString
            
            return $"{whiteSpacesPrefix}{validCalVerString}{whiteSpacesSuffix}";
        }
        
    let whiteSpaceLeadingTrailingValidTagCalVerString =
        gen {
            let! validTagCalVerString = validTagCalVerString
            
            let! whiteSpacesPrefix = genWhiteSpacesString
            let! whiteSpacesSuffix = genWhiteSpacesString
            
            return $"{whiteSpacesPrefix}{validTagCalVerString}{whiteSpacesSuffix}";
        }    
    
    let twoPartCalendarVersionWithSameTimeStamp =
        let genCalVer =
            gen { 
                let! year  = Gen.choose(int Calaf.Domain.Year.lowerYearBoundary, int Calaf.Domain.Year.upperYearBoundary)
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
        
    module Git =        
        let commitMessage =
            gen {
                
                let! choice =
                    Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                                    1, Gen.elements [""; " "]]
                return choice
            }
        
        let commitHash =
            gen {
                return Bogus.Faker().Random.Hash()
            }        
            
        let gitCommitInfo : Gen<GitCommitInfo> =
            gen {
                let! commitMessage = commitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = commitMessage; Hash = commitHash; When = timeStamp }
            }
            
        let calVerGitTagInfo =
            gen {
                let! validCalVerString = validTagCalVerString
                let! maybeCommit = Gen.frequency [
                    1, Gen.constant None
                    3, gitCommitInfo |> Gen.map Some
                ]
                return { Name = validCalVerString; Commit = maybeCommit }                                
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
            
    type internal nullOrWhiteSpaceString =
        static member nullOrWhiteSpaceString() =
            Arb.fromGen Generator.nullOrWhiteSpaceString
            
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
            
    type internal leadingZeroNonYearUInt16String =
        static member leadingZeroNonYearUInt16String() =
            Arb.fromGen Generator.leadingZeroNonYearUInt16String
            
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
            
    type internal validTagCalVerString =
        static member validTagCalVerString() =
            Arb.fromGen Generator.validTagCalVerString
            
    type internal validTwoPartTagCalVerString =
        static member validTwoPartTagCalVerString() =
            Arb.fromGen Generator.validTwoPartTagCalVerString
    
    type internal whiteSpaceLeadingTrailingValidCalVerString =
        static member whiteSpaceLeadingTrailingValidCalVerString() =
            Arb.fromGen Generator.whiteSpaceLeadingTrailingValidCalVerString
            
    type internal whiteSpaceLeadingTrailingValidTagCalVerString =
        static member whiteSpaceLeadingTrailingValidTagCalVerString() =
            Arb.fromGen Generator.whiteSpaceLeadingTrailingValidTagCalVerString
            
    type internal validTagSemVerString =
        static member validTagSemVerString() =
            Arb.fromGen Generator.validTagSemVerString
            
    type internal timeStampIncrement =
        static member timeStampIncrement() =
            Arb.fromGen Generator.timeStampIncrement
            
    module internal Git =            
        type gitCommitInfo =
            static member gitCommitInfo() =
                Arb.fromGen Generator.Git.gitCommitInfo
                
        type calVerGitTagInfo =
            static member calVerGitTagInfo() =
                Arb.fromGen Generator.Git.calVerGitTagInfo