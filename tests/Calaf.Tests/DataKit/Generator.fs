namespace Calaf.Tests.Generator

open FsCheck
open FsCheck.FSharp

open Calaf.Domain.DomainTypes.Values

module Common =
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
        
module Build =
    open Common
    
    let private genBeforeUpperBoundaryUInt16 =
        Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
        
    let private genAlphaPrefix =
        Gen.elements ["alpha"; "ALPHA"; "Alpha"; "AlPhA"; "aLpHa"; "ALPHA"; "alPHA"; "alpHA"; "AlpHA"; "ALpHA"; "ALPhA"; "aLPhA"; "aLpHA"; "aLPha"; "aLPHa"; "AlPha"; "AlpHa"; "aLpha"; "ALpha"; "AlphA"; "alphA"; "ALPHA"]
        
    let private genBetaPrefix =
        Gen.elements ["beta"; "BETA"; "Beta"; "BeTa"; "bEtA"; "bETA"; "beTA"; "betA"; "BEta"; "BETa"]
        
    let private genReleaseCandidatePrefix =
        Gen.elements ["rc"; "RC"; "Rc"; "rC"]
        
    let private genNightlyPrefix =
        Gen.elements ["nightly"; "NIGHTLY"; "Nightly"; "NiGhTlY"; "nIgHtLy"; "NIGHTly"; "nightLY"; "NightLy"; "nighTLY"; "NiGhTLy"; "nIGhTly"; "niGhtly"; "niGhTly"; "niGhTLy"; "nIghtly"; "NightLY"; "NIghtly"; "NigHtly"; "NightLy"; "NightlY"; "nIGHTLY"; "niGHTLY"; "niGhTLY"; "niGhTlY"; "niGHTly"; "niGHTLy"; "niGHTLY"; "nIGhTLY"; "nIGHTly"; "nIGHTLy"; "nIGHTLY"; "NIGhTLY"]

    let private genAlphaBuild : Gen<AlphaBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }

    let private genBetaBuild : Gen<BetaBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }
        
    let private genReleaseCandidateBuild : Gen<ReleaseCandidateBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }
        
    let private genNightlyBuild =
        gen {
            let! day = genDay
            let! number = genUInt16
            return { Day = day; Number = number } 
        }

    let private genAlphaNightlyBuild =
        gen {
            let! alphaBuild = genAlphaBuild
            let! nightlyBuild = genNightlyBuild
            return (alphaBuild, nightlyBuild)
        }
        
    let private genBetaNightlyBuild =
        gen {
            let! betaBuild = genBetaBuild
            let! nightlyBuild = genNightlyBuild
            return (betaBuild, nightlyBuild)
        }
        
    let private genReleaseCandidateNightlyBuild =
        gen {
            let! rcBuild = genReleaseCandidateBuild
            let! nightlyBuild = genNightlyBuild
            return (rcBuild, nightlyBuild)
        }
        
    let wrongString =
        let letterChars = ['a'..'z'] @ ['A'..'Z']
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        gen {
            let! shuffle = Gen.shuffle (letterChars @ specialChars)
            let! length = Gen.choose(64, 512)
            let! chars = Gen.arrayOfLength length (Gen.elements shuffle)
            return System.String(chars)
        }
        
    let wrongString2 =            
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        gen {
            let! length = Gen.choose(64, 512)
            let! chars = Gen.arrayOfLength length (Gen.elements specialChars)
            return System.String(chars)
        }
        
    let genContainingBadString (goodString: string) =
        gen {
            let! specialCharacters = wrongString2
            let! leadingWhiteSpaces = genWhiteSpacesString
            let! choice = Gen.frequency [
                1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{goodString}"
                1, Gen.constant $"{goodString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                1, Gen.constant $"{goodString}{wrongString}{goodString}"
                1, Gen.constant $"{specialCharacters}{goodString}{specialCharacters}"
                1, Gen.constant $"{specialCharacters}{goodString}"
                1, Gen.constant $"{goodString}{specialCharacters}"
                1, Gen.constant $"{goodString}{leadingWhiteSpaces}"
                1, Gen.constant $"{leadingWhiteSpaces}{goodString}"
                1, Gen.constant $"{leadingWhiteSpaces}{goodString}{leadingWhiteSpaces}"
                1, Gen.constant $"{goodString}{goodString}"
                1, Gen.constant $"{goodString}{goodString}{goodString}"                    
            ]
            return choice
        }
        
    module Alpha =
        let String =
            gen {
                let! alphaPrefix = genAlphaPrefix
                let! alpha = genAlphaBuild
                return $"{alphaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{alpha.Number}"
            }
            
        let BadString=
            gen {
                let! alphaString = String
                let! badString = genContainingBadString alphaString
                return badString
            }
            
        let Accidental =
            genAlphaBuild
            
        let AccidentalBuild =
            gen {
                let! a = genAlphaBuild
                return Build.Alpha a
            }
            
        let AccidentalBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, Accidental |> Gen.map Some
            ]
            
        let NoUpperBoundaryAccidentalBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Alpha { Number = number }                
            }
        
    module Beta =
        let betaString =
            gen {
                let! betaPrefix = genBetaPrefix
                let! beta = genBetaBuild
                return $"{betaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{beta.Number}"
            }
            
        let containingBetaBadString =
            gen {
                let! betaString = betaString
                let! badString = genContainingBadString betaString
                return badString
            }
            
        let beta =
            genBetaBuild
            
        let betaBuild =
            gen {
                let! b = genBetaBuild
                return Build.Beta b
            }
            
        let betaBuildOption =
            gen {
                let! choice = Gen.frequency [
                    1, Gen.constant None
                    3, betaBuild |> Gen.map Some
                ]
                return choice
            }
            
        let numberNoUpperBoundaryBetaBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Beta { Number = number }                
            }
            
    module ReleaseCandidate =
        let rcString =
            gen {
                let! rcPrefix = genReleaseCandidatePrefix
                let! rc = genReleaseCandidateBuild
                return $"{rcPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{rc.Number}"
            }
            
        let containingReleaseCandidateBadString =
            gen {
                let! betaString = rcString
                let! badString = genContainingBadString betaString
                return badString
            }
            
        let rc =
            genReleaseCandidateBuild
            
        let rcBuild =
            gen {
                let! rc = genReleaseCandidateBuild
                return Build.ReleaseCandidate rc
            }
            
        let rcBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, rcBuild |> Gen.map Some
            ]
            
        let numberNoUpperBoundaryReleaseCandidateBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.ReleaseCandidate { Number = number }                
            }
            
    module Nightly =
        let nightlyString =
            gen {
                let! nightlyPrefix = genNightlyPrefix
                let! nightly = genNightlyBuild
                return $"{Calaf.Domain.Build.NightlyZeroPrefix}{Calaf.Domain.Build.NightlyZeroBuildTypeDivider}{nightlyPrefix}{Calaf.Domain.Build.BuildTypeDayDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            }
            
        let containingNightlyBadString =
            gen {
                let! nightlyString = nightlyString
                let! specialCharacters = wrongString2
                let! leadingWhiteSpaces = genWhiteSpacesString
                let! choice = Gen.frequency [
                    1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                    1, Gen.constant $"{nightlyString}{wrongString}{nightlyString}"
                    1, Gen.constant $"{specialCharacters}{nightlyString}{specialCharacters}"
                    1, Gen.constant $"{specialCharacters}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{specialCharacters}"
                    1, Gen.constant $"{nightlyString}{leadingWhiteSpaces}"
                    1, Gen.constant $"{leadingWhiteSpaces}{nightlyString}"
                    1, Gen.constant $"{leadingWhiteSpaces}{nightlyString}{leadingWhiteSpaces}"
                    1, Gen.constant $"{nightlyString}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{nightlyString}{nightlyString}"                    
                ]
                return choice
            }
        
        let nightly =
            genNightlyBuild
            
        let nightlyBuild =
            gen {
                let! n = genNightlyBuild
                return Build.Nightly n
            }
            
        let nightlyBuildOption =
            gen {
                let! choice = Gen.frequency [
                    1, Gen.constant None
                    3, nightlyBuild |> Gen.map Some
                ]
                return choice
            }
            
        let numberNoUpperBoundaryNightlyBuild =
            gen {
                let! day = genDay
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Nightly { Day = day; Number = number }                
            }

    module AlphaNightly =
        let String =
            gen {
                let! alphaPrefix = genAlphaPrefix
                let! alpha, nightly = genAlphaNightlyBuild
                return $"{alphaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{alpha.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let BadString =
            gen {
                let! alphaNightlyString = String
                let! badString = genContainingBadString alphaNightlyString
                return badString
            }
            
        let AccidentalBuild =
            gen {
                let! an = genAlphaNightlyBuild
                return Build.AlphaNightly an
            }
            
        let AccidentalBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, AccidentalBuild |> Gen.map Some
            ]
        
        let NoUpperBoundaryAccidentalBuild =
            gen {
                let! alphaNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.AlphaNightly({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }
            
    module BetaNightly =
        let betaNightlyString =
            gen {
                let! betaPrefix = genBetaPrefix
                let! beta, nightly = genBetaNightlyBuild
                return $"{betaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{beta.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let containingBetaNightlyBadString =
            gen {
                let! betaNightlyString = betaNightlyString
                let! badString = genContainingBadString betaNightlyString
                return badString
            }
            
        let betaNightlyBuild =
            gen {
                let! bn = genBetaNightlyBuild
                return Build.BetaNightly bn
            }
            
        let betaNightlyBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, betaNightlyBuild |> Gen.map Some
            ]
        
        let numberNoUpperBoundaryBetaNightlyBuild =
            gen {
                let! betaNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.BetaNightly({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }
            
    module ReleaseCandidateNightly =
        let rcNightlyString =
            gen {
                let! rcPrefix = genReleaseCandidatePrefix
                let! rc, nightly = genReleaseCandidateNightlyBuild
                return $"{rcPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{rc.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let containingReleaseCandidateNightlyBadString =
            gen {
                let! rcNightlyString = rcNightlyString
                let! badString = genContainingBadString rcNightlyString
                return badString
            }
            
        let rcNightlyBuild =
            gen {
                let! rcn = genReleaseCandidateNightlyBuild
                return Build.ReleaseCandidateNightly rcn
            }
            
        let rcNightlyBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, rcNightlyBuild |> Gen.map Some
            ]
        
        let numberNoUpperBoundaryReleaseCandidateNightlyBuild =
            gen {
                let! rcNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.ReleaseCandidateNightly({ Number = rcNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }
            
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
        
module Month =
    open Common
    
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
        
module Year =
    open Common
    
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
        
module CalendarVersion =
    open Common
    
    module Stable =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}"
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}"
            }        
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
            
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
        
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                return { Year = year; Month = month; Micro = None; Build = None }
            }
            
        let Micro =
            gen {
                let! short = Short
                let! micro = validMicroUInt32
                return { short with Micro = Some micro }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]

    module Alpha =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! alphaString = Build.Alpha.String
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{alphaString}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! alphaString = Build.Alpha.String
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{alphaString}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
            
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = Gen.frequency [
                    1, ShortString
                    1, MicroString
                ]
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! alphaBuild = Build.Alpha.AccidentalBuild
                return { shortCalendarVersion with Build = Some alphaBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! alphaBuild = Build.Alpha.AccidentalBuild
                return { microCalendarVersion with Build = Some alphaBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]
            
    module Beta =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! betaBuild = Build.Beta.betaString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaBuild}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! betaBuild = Build.Beta.betaString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaBuild}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
            
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = Gen.frequency [
                    1, ShortString
                    1, MicroString
                ]
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! betaBuild = Build.Beta.betaBuild
                return { shortCalendarVersion with Build = Some betaBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! betaBuild = Build.Beta.betaBuild
                return { microCalendarVersion with Build = Some betaBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]
                
    module ReleaseCandidate =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! rcBuild = Build.ReleaseCandidate.rcString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{rcBuild}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! rcBuild = Build.ReleaseCandidate.rcString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{rcBuild}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
            
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = Gen.frequency [
                    1, ShortString
                    1, MicroString
                ]
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! rcBuild = Build.ReleaseCandidate.rcBuild
                return { shortCalendarVersion with Build = Some rcBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! rcBuild = Build.ReleaseCandidate.rcBuild
                return { microCalendarVersion with Build = Some rcBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]
        
    module Nightly =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! nightlyBuild = Build.Nightly.nightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{nightlyBuild}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! nightlyBuild = Build.Nightly.nightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{nightlyBuild}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
            
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! nightlyBuild = Build.Nightly.nightlyBuild
                return { shortCalendarVersion with Build = Some nightlyBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! nightlyBuild = Build.Nightly.nightlyBuild
                return { microCalendarVersion with Build = Some nightlyBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]

    module AlphaNightly =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! alphaNightlyString = Build.AlphaNightly.String
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{alphaNightlyString}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! alphaNightlyString = Build.AlphaNightly.String
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{alphaNightlyString}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
                
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! alphaNightlyBuild = Build.AlphaNightly.AccidentalBuild
                return { shortCalendarVersion with Build = Some alphaNightlyBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! alphaNightlyBuild = Build.AlphaNightly.AccidentalBuild
                return { microCalendarVersion with Build = Some alphaNightlyBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]
                
    module BetaNightly =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaNightlyBuild}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaNightlyBuild}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
                
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyBuild
                return { shortCalendarVersion with Build = Some betaNightlyBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyBuild
                return { microCalendarVersion with Build = Some betaNightlyBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]
                
    module ReleaseCandidateNightly =
        let ShortString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! rcNightlyBuild = Build.ReleaseCandidateNightly.rcNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{rcNightlyBuild}"                 
            }
            
        let MicroString =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! micro = validMicroUInt32
                let! rcNightlyBuild = Build.ReleaseCandidateNightly.rcNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthMicroDivider}{micro}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{rcNightlyBuild}"                 
            }
            
        let String =
            Gen.frequency
                [ 1, ShortString
                  1, MicroString ]
                
        let TagStrictString =
            gen {
                let prefix = Calaf.Domain.Version.tagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let TagString =
            gen {
                let! prefix = genTagVersionPrefix
                let! version = String
                return $"{prefix}{version}"
            }
            
        let Short =
            gen {
                let! shortCalendarVersion = Stable.Short
                let! rcNightlyBuild = Build.ReleaseCandidateNightly.rcNightlyBuild
                return { shortCalendarVersion with Build = Some rcNightlyBuild }
            }
            
        let Micro =
            gen {
                let! microCalendarVersion = Stable.Micro
                let! rcNightlyBuild = Build.ReleaseCandidateNightly.rcNightlyBuild
                return { microCalendarVersion with Build = Some rcNightlyBuild }
            }
            
        let Accidental =
            Gen.frequency
                [ 1, Short
                  1, Micro ]

    let ShortString =
        Gen.frequency
            [ 1, Stable.ShortString
              1, Alpha.ShortString
              1, Beta.ShortString
              1, ReleaseCandidate.ShortString
              1, Nightly.ShortString
              1, AlphaNightly.ShortString
              1, BetaNightly.ShortString
              1, ReleaseCandidateNightly.ShortString ]
            
    let MicroString =
        Gen.frequency
            [ 1, Stable.MicroString
              1, Alpha.MicroString
              1, Beta.MicroString
              1, ReleaseCandidate.MicroString
              1, Nightly.MicroString
              1, AlphaNightly.MicroString
              1, BetaNightly.MicroString
              1, ReleaseCandidateNightly.MicroString ]
            
    let String =
        Gen.frequency
            [ 1, Stable.String
              1, Alpha.String
              1, Beta.String
              1, ReleaseCandidate.String
              1, Nightly.String
              1, AlphaNightly.String
              1, BetaNightly.String
              1, ReleaseCandidateNightly.String ]
            
    let ShortTagString =
        gen {
            let! prefix = genTagVersionPrefix
            let! version = ShortString
            return $"{prefix}{version}"
        }
        
    let MicroTagString =
        gen {
            let! prefix = genTagVersionPrefix
            let! version = MicroString
            return $"{prefix}{version}"
        }

    let TagStrictString =
        Gen.frequency
            [ 1, Stable.TagStrictString
              1, Alpha.TagStrictString
              1, Beta.TagStrictString
              1, ReleaseCandidate.TagStrictString
              1, Nightly.TagStrictString
              1, AlphaNightly.TagStrictString
              1, BetaNightly.TagStrictString
              1, ReleaseCandidateNightly.TagStrictString ]
            
    let TagString =
        Gen.frequency
            [ 1, Stable.TagString
              1, Alpha.TagString
              1, Beta.TagString
              1, ReleaseCandidate.TagString
              1, Nightly.TagString
              1, AlphaNightly.TagString
              1, ReleaseCandidateNightly.TagString ]

    let AccidentalShort =
        Gen.frequency
            [ 1, Stable.Short
              1, Alpha.Short
              1, Beta.Short
              1, ReleaseCandidate.Short
              1, Nightly.Short
              1, AlphaNightly.Short
              1, BetaNightly.Short
              1, ReleaseCandidateNightly.Short ]
            
    let AccidentalMicro =
        Gen.frequency
            [ 1, Stable.Micro
              1, Alpha.Micro
              1, Beta.Micro
              1, ReleaseCandidate.Micro
              1, Nightly.Micro
              1, AlphaNightly.Micro
              1, BetaNightly.Micro
              1, ReleaseCandidateNightly.Micro ]
            
    let AccidentalPreReleases =
        Gen.frequency
            [ 1, Alpha.Short
              1, Alpha.Micro
              1, Beta.Short
              1, Beta.Micro
              1, ReleaseCandidate.Short
              1, ReleaseCandidate.Micro
              1, Nightly.Short
              1, Nightly.Micro
              1, AlphaNightly.Short
              1, AlphaNightly.Micro
              1, BetaNightly.Short
              1, BetaNightly.Micro
              1, ReleaseCandidateNightly.Short
              1, ReleaseCandidateNightly.Micro ]
            
    let Accidental =
        Gen.frequency
            [ 1, Stable.Short
              1, Stable.Micro
              1, Alpha.Short
              1, Alpha.Micro
              1, Beta.Short
              1, Beta.Micro
              1, ReleaseCandidate.Short
              1, ReleaseCandidate.Micro
              1, Nightly.Short
              1, Nightly.Micro
              1, AlphaNightly.Short
              1, AlphaNightly.Micro
              1, BetaNightly.Short
              1, BetaNightly.Micro
              1, ReleaseCandidateNightly.Short
              1, ReleaseCandidateNightly.Micro ]
        
    let AccidentalsArray =
        gen {
            let! smallCount = Gen.choose(1, 50)
            let! middleCount = Gen.choose(51, 1000)            
            let! bigCount = Gen.choose(1001, 25_000)            
            let! choice = Gen.frequency [
                3, Gen.arrayOfLength smallCount  Accidental
                2, Gen.arrayOfLength middleCount Accidental
                1, Gen.arrayOfLength bigCount    Accidental
            ]
            return choice
        }
        
    let WhiteSpaceLeadingTrailingString =
        gen {
            let! calVerStr = String
            
            let! whiteSpacesPrefix = genWhiteSpacesString
            let! whiteSpacesSuffix = genWhiteSpacesString
            
            return $"{whiteSpacesPrefix}{calVerStr}{whiteSpacesSuffix}";
        }
        
    let WhiteSpaceLeadingTrailingTagString =
        gen {
            let! calVerStr = TagString
            
            let! whiteSpacesPrefix = genWhiteSpacesString
            let! whiteSpacesSuffix = genWhiteSpacesString
            
            return $"{whiteSpacesPrefix}{calVerStr}{whiteSpacesSuffix}";
        }
        
module SematicVersion =
    open Common
    
    let semanticVersion =
        let genBigSemVer =
            gen {
                let! major = Gen.choose64(1000, int64 System.UInt32.MaxValue)
                let! minor = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                let! patch = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                return uint32 major, uint32 minor, uint32 patch
            }
        let genSmallSemVer =
            gen {
                let! major = Gen.choose(0, 999)
                let! minor = Gen.choose(13, 99999)
                let! patch = Gen.choose(1, 99999)
                return uint32 major, uint32 minor, uint32 patch
            }
        gen {
            let! major, minor, patch = Gen.frequency [
                1, genBigSemVer
                1, genSmallSemVer
            ]
            return { Major = major; Minor = minor; Patch = patch }
        }
        
    let semanticVersion2 =
        gen {
            let! semanticVersion = semanticVersion
            return semanticVersion, $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}"
        }
        
    let semanticVersionStr =
        gen {
            let! semanticVersion = semanticVersion
            return $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}"
        }
        
    let semanticVersionTagStr =
        gen {
            let! validPrefix = genTagVersionPrefix
            let! semVer = semanticVersionStr
            return $"{validPrefix}{semVer}"
        }
        
module Git =
    open Common
    
    open Calaf.Contracts
    open Calaf.Domain.DomainTypes.Entities
    
    let branchName =            
        Gen.frequency [ 1, SematicVersion.semanticVersionTagStr
                        1, Gen.elements [ "master"; "main"; "develop"; "feature"; "bugfix"; "release" ]]            

    let branchNameOrNone =
        Gen.frequency [ 1, Gen.constant None
                        3, branchName |> Gen.map Some ]
        
        
    let commitText =
        Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                        1, Gen.elements [""; " "]]

    let commitHash =
        gen {
            return Bogus.Faker().Random.Hash()
        }
        
    module Commit =
        let private leftBracketOrEmpty scope =
            if System.String.IsNullOrWhiteSpace scope
            then System.String.Empty
            else "("
            
        let private rightBracketOrEmpty scope =
            if System.String.IsNullOrWhiteSpace scope
            then System.String.Empty
            else ")"
            
        let private commitEntry breakingChange =
            gen {
                let! desc = commitText
                let! scope =
                    Gen.frequency [ 3, Gen.map (fun word -> $"{word}") (Gen.constant (Bogus.Faker().Lorem.Word()))
                                    1, Gen.elements [""; " "]]
                let breakingChange =
                    if breakingChange
                    then Calaf.Domain.Commit.BreakingChange
                    else System.String.Empty                    
                let text =
                    $"{Calaf.Domain.Commit.FixPrefix}{leftBracketOrEmpty scope}{scope}{rightBracketOrEmpty scope}{breakingChange}{Calaf.Domain.Commit.EndOfPattern} {desc}"
                let desc =
                    if System.String.IsNullOrWhiteSpace desc
                    then None
                    else Some desc
                let scope =
                    if System.String.IsNullOrWhiteSpace scope
                    then None
                    else Some scope
                return (text, desc, scope)
            }
            
        let private otherCommitMessage =
            gen {                
                let! text = commitText                    
                if System.String.IsNullOrWhiteSpace text
                then return (text, CommitMessage.Other None)
                else return (text, CommitMessage.Other (Some text))
            }
            
        let private fixNonBreakingChangeCommitMessage =
            gen {
                let nonBreakingChange = false
                let! text, desc, scope = commitEntry nonBreakingChange                
                return (text, CommitMessage.Fix (nonBreakingChange, scope, desc))
            }
            
        let private fixBreakingChangeCommitMessage =
            gen {
                let breakingChange = true
                let! text, desc, scope = commitEntry breakingChange                
                return (text, CommitMessage.Fix (breakingChange, scope, desc))
            }
            
        let private featNonBreakingChangeCommitMessage =
            gen {
                let nonBreakingChange = false
                let! text, desc, scope = commitEntry nonBreakingChange                
                return (text, CommitMessage.Feature (nonBreakingChange, scope, desc))
            }
            
        let private featBreakingChangeCommitMessage =
            gen {
                let breakingChange = true
                let! text, desc, scope = commitEntry breakingChange                
                return (text, CommitMessage.Feature (breakingChange, scope, desc))
            }
            
        let otherCommit: Gen<Commit> =
            gen {
                let! text, message = otherCommitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = message
                         Text    = text
                         Hash    = commitHash
                         When    = timeStamp }
            }
            
        let fixNonBreakingChangeCommit: Gen<Commit> =
            gen {
                let! text, message = fixNonBreakingChangeCommitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = message
                         Text    = text
                         Hash    = commitHash
                         When    = timeStamp }
            }
            
        let fixBreakingChangeCommit: Gen<Commit> =
            gen {
                let! text, message = fixBreakingChangeCommitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = message
                         Text    = text
                         Hash    = commitHash
                         When    = timeStamp }
            }
            
        let featNonBreakingChangeCommit: Gen<Commit> =
            gen {
                let! text, message = featNonBreakingChangeCommitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = message
                         Text    = text
                         Hash    = commitHash
                         When    = timeStamp }
            }
            
        let featBreakingChangeCommit: Gen<Commit> =
            gen {
                let! text, message = featBreakingChangeCommitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = message
                         Text    = text
                         Hash    = commitHash
                         When    = timeStamp }
            }
        
    let gitCommitInfo : Gen<GitCommitInfo> =
        gen {
            let! commitText = commitText
            let! commitHash = commitHash
            let! timeStamp = genValidDateTimeOffset
            return { Text = commitText; Hash = commitHash; When = timeStamp }
        }
        
    let gitSignatureInfo : Gen<GitSignatureInfo> =
        gen {
            let! email = Gen.constant (Bogus.Faker().Person.Email)
            let! name = Gen.constant (Bogus.Faker().Person.FullName)
            let! timeStamp = Gen.constant (Bogus.Faker().Date.BetweenOffset(System.DateTimeOffset.MinValue, System.DateTimeOffset.MaxValue))
            return { Email = email; Name = name; When = timeStamp }
        }
        
    let gitTagInfo =
        gen {
            let! commit = gitCommitInfo
            let name    = commit.Text 
            let! commit = Gen.frequency [
                1, Gen.constant None
                3, Gen.constant (Some commit)
            ]
            return { Name = name; Commit = commit }
        }        
            
    let calVerGitTagInfo =
        gen {
            let! validCalVerString = CalendarVersion.TagString
            let! maybeCommit = Gen.frequency [
                1, Gen.constant None
                3, gitCommitInfo |> Gen.map Some
            ]
            return { Name = validCalVerString; Commit = maybeCommit }                                
        }
        
    let semVerGitTagInfo =
        gen {
            let! validSemVerString = SematicVersion.semanticVersionTagStr
            let! maybeCommit = Gen.frequency [
                1, Gen.constant None
                3, gitCommitInfo |> Gen.map Some
            ]
            return { Name = validSemVerString; Commit = maybeCommit }                       
        }
        
    let unversionedTagName =
        Gen.frequency [
                3, nonNumericString
                2, invalidThreePartString
                1, nullOrWhiteSpaceString
            ]
        
    let malformedGitTagInfo =
        gen {
            let! unversionedTagName = unversionedTagName
            let! maybeCommit = Gen.frequency [
                1, Gen.constant None
                3, gitCommitInfo |> Gen.map Some
            ]
            return { Name = unversionedTagName; Commit = maybeCommit }
        }
        
    let calendarVersionOrSemanticVersionGitTagInfo =
        gen {
            let! tagName = Gen.frequency [
                3, CalendarVersion.TagString
                1, SematicVersion.semanticVersionTagStr
            ]
            let! commit = gitCommitInfo |> Gen.map Some
            return { Name = tagName; Commit = commit }
        }
        
    let randomGitTagInfo =
        gen {
            let! choice = Gen.frequency [
                1, malformedGitTagInfo
                3, calVerGitTagInfo
                2, semVerGitTagInfo
            ]
            return choice
        }
        
    let commitOrNone =
        Gen.frequency [
            1, Gen.constant None
            2, Commit.otherCommit |> Gen.map Some
            2, Commit.fixNonBreakingChangeCommit |> Gen.map Some
            2, Commit.fixBreakingChangeCommit |> Gen.map Some
            2, Commit.featNonBreakingChangeCommit |> Gen.map Some
            2, Commit.featBreakingChangeCommit |> Gen.map Some
        ]
        
    let calendarVersionTag : Gen<Tag> =            
        gen {
            // TODO: Rewrite generator!
            let! calendarVersion = CalendarVersion.Accidental
            let tagNameSb = System.Text.StringBuilder($"{calendarVersion.Year}.{calendarVersion.Month}")
            let tagNameSb = if calendarVersion.Micro.IsSome then tagNameSb.Append calendarVersion.Micro.Value else tagNameSb 
            let! maybeCommit = commitOrNone
            return Tag.Versioned (tagNameSb.ToString(), (calendarVersion |> CalVer), maybeCommit)
        }
        
    let sematicVersionTag : Gen<Tag> =            
        gen {
            let! semanticVersion, stringEquivalent = SematicVersion.semanticVersion2
            let! maybeCommit = commitOrNone
            return Tag.Versioned (stringEquivalent, (SemVer semanticVersion), maybeCommit)
        }
        
    let unversionedTag : Gen<Tag> =            
        gen {
            let! tagName = unversionedTagName
            return Tag.Unversioned tagName
        }
        
    let calendarVersionsTagsArray =
         gen {
            let! smallCount = Gen.choose(1, 50)
            let! middleCount = Gen.choose(51, 100)            
            let! bigCount = Gen.choose(101, 250)            
            let! choice = Gen.frequency [
                7, Gen.arrayOfLength smallCount  calendarVersionTag
                2, Gen.arrayOfLength middleCount calendarVersionTag
                1, Gen.arrayOfLength bigCount    calendarVersionTag
            ]
            return choice
        }
         
    let semanticVersionsTagsArray =
        gen {
            let! smallCount  = Gen.choose(1, 50)
            let! middleCount = Gen.choose(51, 100)            
            let! bigCount    = Gen.choose(101, 250)            
            let! choice = Gen.frequency [
                7, Gen.arrayOfLength smallCount  sematicVersionTag
                2, Gen.arrayOfLength middleCount sematicVersionTag
                1, Gen.arrayOfLength bigCount    sematicVersionTag
            ]
            return choice
        }
        
    let unversionedTagsArray =
        gen {
            let! smallCount  = Gen.choose(1, 50)
            let! middleCount = Gen.choose(51, 100)            
            let! bigCount    = Gen.choose(101, 250)            
            return! Gen.frequency [
                7, Gen.arrayOfLength smallCount  unversionedTag
                2, Gen.arrayOfLength middleCount unversionedTag
                1, Gen.arrayOfLength bigCount    unversionedTag
            ]
        }
        
    let semanticVersionsAndUnversionedTagsArray =
        gen {
            let! semanticVersionsTags = semanticVersionsTagsArray
            let! unversionedTags = unversionedTagsArray
            return Array.append semanticVersionsTags unversionedTags                
        }
        
    let baseGitRepositoryInfo =
        gen {
            let! dir = directoryPathString
            let! unborn = genBool
            let! detached = genBool
            let! branch = branchNameOrNone
            let! commit = Gen.frequency [ 1, Gen.constant None; 3, gitCommitInfo |> Gen.map Some ]
            let! signature = Gen.frequency [1, Gen.constant None; 3, gitSignatureInfo |> Gen.map Some ]
            let! dirty = genBool
            let! tags = Gen.listOf gitTagInfo
            return {
                Directory = dir
                Unborn = unborn
                Detached = detached
                CurrentBranch = branch
                CurrentCommit = commit
                Signature = signature
                Dirty = dirty
                Tags = tags
            }
    }
        
module Contracts =
    open System.Xml.Linq
    
    open Calaf.Contracts
    open Calaf.Domain.Project.XmlSchema
    open Calaf.Application
      
    let private projectFileExtension = 
        Bogus.Faker().Random.ArrayElement(
            [| Calaf.Domain.Language.FSharpProjExtension; Calaf.Domain.Language.CSharpProjExtension |])

    let projectXElement (version : string option) : XElement =
         match version with
         | Some v -> 
             XElement(XName.Get(ProjectXElementName),
                XElement(XName.Get(PropertyGroupXElementName),
                    XElement(XName.Get(VersionXElementName), v)))
         | None ->
            XElement(XName.Get(ProjectXElementName),
                XElement(XName.Get(PropertyGroupXElementName)))
         
    let projectXmlFileInfo (rootDir: string, version : string option) : ProjectXmlFileInfo =
        let dir =
            if Bogus.Faker().Random.Bool()
            then rootDir + Bogus.Faker().System.DirectoryPath()
            else rootDir
        let name = Bogus.Faker().System.FileName()
        let ext = projectFileExtension
        let absolutePath = dir + "/" + name + ext
        {
            Name = name
            Directory = dir
            Extension = ext
            AbsolutePath = absolutePath
            Content = projectXElement version
        }
         
    let directoryInfo () : DirectoryInfo =
        let dir = Bogus.Faker().System.DirectoryPath()
        let projects =
            Bogus.Faker().Make<ProjectXmlFileInfo>(
                int (Bogus.Faker().Random.Byte(1uy, System.Byte.MaxValue)),
                fun (_: int) -> projectXmlFileInfo (dir, Some "2025.7")) |> Seq.toList            
        { Directory = dir; Projects = projects }
        
    let makeSettings () : MakeSettings =
        let projectPattern = MakeSettings.tryCreateDotNetXmlFilePattern "*.csproj"
        let tagCount = MakeSettings.tryCreateTagCount 10uy
        match projectPattern, tagCount with
        | Ok pattern, Ok count -> 
            { ProjectsSearchPattern = pattern; TagsToLoad = count }
        | _ -> failwith "Failed to create test settings"