namespace Calaf.Tests

open FsCheck.FSharp

open Calaf.Domain.DomainTypes.Values

module Generator =
    let private genBool =
        Gen.elements [true; false]
        
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
        let private genBetaPrefix =
            Gen.elements ["beta"; "BETA"; "Beta"; "BeTa"; "bEtA"; "bETA"; "beTA"; "betA"; "BEta"; "BETa"]
            
        let private genNightlyPrefix =
            Gen.elements ["nightly"; "NIGHTLY"; "Nightly"; "NiGhTlY"; "nIgHtLy"; "NIGHTly"; "nightLY"; "NightLy"; "nighTLY"; "NiGhTLy"; "nIGhTly"; "niGhtly"; "niGhTly"; "niGhTLy"; "nIghtly"; "NightLY"; "NIghtly"; "NigHtly"; "NightLy"; "NightlY"; "nIGHTLY"; "niGHTLY"; "niGhTLY"; "niGhTlY"; "niGHTly"; "niGHTLy"; "niGHTLY"; "nIGhTLY"; "nIGHTly"; "nIGHTLy"; "nIGHTLY"; "NIGhTLY"]
            
        let private genBetaBuild =
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
            
        let private genBetaNightlyBuild =
            gen {
                let! betaBuild = genBetaBuild
                let! nightlyBuild = genNightlyBuild
                return (betaBuild, nightlyBuild)
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
                    let! specialCharacters = wrongString2
                    let! leadingWhiteSpaces = genWhiteSpacesString
                    let! choice = Gen.frequency [
                        1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{betaString}"
                        1, Gen.constant $"{betaString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                        1, Gen.constant $"{betaString}{wrongString}{betaString}"
                        1, Gen.constant $"{specialCharacters}{betaString}{specialCharacters}"
                        1, Gen.constant $"{specialCharacters}{betaString}"
                        1, Gen.constant $"{betaString}{specialCharacters}"
                        1, Gen.constant $"{betaString}{leadingWhiteSpaces}"
                        1, Gen.constant $"{leadingWhiteSpaces}{betaString}"
                        1, Gen.constant $"{leadingWhiteSpaces}{betaString}{leadingWhiteSpaces}"
                        1, Gen.constant $"{betaString}{betaString}"
                        1, Gen.constant $"{betaString}{betaString}{betaString}"                    
                    ]
                    return choice
                }
                
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
                    let! number = Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
                    return Build.Beta { Number = number }                
                }
                
        module Nightly =
            let nightlyString =
                gen {
                    let! nightlyPrefix = genNightlyPrefix
                    let! nightly = genNightlyBuild
                    return $"{nightlyPrefix}{Calaf.Domain.Build.BuildTypeDayDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
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
                    let! number = Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
                    return Build.Nightly { Day = day; Number = number }                
                }
                
        module BetaNightly =
            let betaNightlyString =
                gen {
                    let! betaPrefix = genBetaPrefix
                    let! beta, nightly = genBetaNightlyBuild
                    return $"{betaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{beta.Number}{Calaf.Domain.Build.BetaNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
                } 
            
            let containingBetaNightlyBadString =
                gen {
                    let! betaNightlyString = betaNightlyString
                    let! specialCharacters = wrongString2
                    let! leadingWhiteSpaces = genWhiteSpacesString
                    let! choice = Gen.frequency [
                        1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{betaNightlyString}"
                        1, Gen.constant $"{betaNightlyString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                        1, Gen.constant $"{betaNightlyString}{wrongString}{betaNightlyString}"
                        1, Gen.constant $"{specialCharacters}{betaNightlyString}{specialCharacters}"
                        1, Gen.constant $"{specialCharacters}{betaNightlyString}"
                        1, Gen.constant $"{betaNightlyString}{specialCharacters}"
                        1, Gen.constant $"{betaNightlyString}{leadingWhiteSpaces}"
                        1, Gen.constant $"{leadingWhiteSpaces}{betaNightlyString}"
                        1, Gen.constant $"{leadingWhiteSpaces}{betaNightlyString}{leadingWhiteSpaces}"
                        1, Gen.constant $"{betaNightlyString}{betaNightlyString}"
                        1, Gen.constant $"{betaNightlyString}{betaNightlyString}{betaNightlyString}"                    
                    ]
                    return choice
                }
                
            let betaNightlyBuild =
                gen {
                    let! bn = genBetaNightlyBuild
                    return Build.BetaNightly bn
                }
                
            let betaNightlyBuildOption =
                gen {
                    let! choice = Gen.frequency [
                        1, Gen.constant None
                        3, betaNightlyBuild |> Gen.map Some
                    ]
                    return choice
                }
            
            let numberNoUpperBoundaryBetaNightlyBuild =
                gen {
                    let! betaNumber = Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
                    let! nightlyDay = genDay
                    let! nightlyNumber = Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
                    return Build.BetaNightly({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })                
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
                let! lowerThanAllowed   = Gen.choose(1, int Calaf.Domain.Year.LowerYearBoundary - 1)
                let! greaterThanAllowed = Gen.choose(int Calaf.Domain.Year.UpperYearBoundary + 1, int System.UInt16.MaxValue)
                let! year = Gen.frequency [
                    1, Gen.constant lowerThanAllowed
                    1, Gen.constant greaterThanAllowed
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
        let calendarVersionShortBetaBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! betaBuild = Build.Beta.betaString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaBuild}"                 
            }
            
        let calendarVersionPatchBetaBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                let! betaBuild = Build.Beta.betaString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthPatchDivider}{patch}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaBuild}"                 
            }
            
        let calendarVersionBetaBuildStr =
            gen {
                let! choice = Gen.frequency [
                    1, calendarVersionShortBetaBuildStr
                    1, calendarVersionPatchBetaBuildStr
                ]
                return choice
            }
            
        let calendarVersionShortBetaNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaNightlyBuild}"                 
            }
            
        let calendarVersionPatchBetaNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthPatchDivider}{patch}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{betaNightlyBuild}"                 
            }
            
        let calendarVersionNightlyBetaBuildStr =
            gen {
                let! choice = Gen.frequency [
                    1, calendarVersionShortBetaNightlyBuildStr
                    1, calendarVersionPatchBetaNightlyBuildStr
                ]
                return choice
            }
            
        let calendarVersionShortNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! nightlyBuild = Build.Nightly.nightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{nightlyBuild}"                 
            }
            
        let calendarVersionPatchNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                let! nightlyBuild = Build.Nightly.nightlyString
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthPatchDivider}{patch}{Calaf.Domain.Version.CalendarVersionBuildTypeDivider}{nightlyBuild}"                 
            }
            
        let calendarVersionNightlyBuildStr =
            gen {
                let! choice = Gen.frequency [
                    1, calendarVersionShortNightlyBuildStr
                    1, calendarVersionPatchNightlyBuildStr
                ]
                return choice
            }
            
        let calendarVersionShortStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}"
            }
            
        let calendarVersionPatchStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                return $"{year}{Calaf.Domain.Version.YearMonthDivider}{month}{Calaf.Domain.Version.MonthPatchDivider}{patch}"
            }        
            
        let calendarVersionStr =
            gen {            
                let! calendarVersion = Gen.frequency [
                    1, calendarVersionPatchStr
                    1, calendarVersionShortStr
                ]
                return calendarVersion
            }
        
        let calendarVersionShort =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                return { Year = year; Month = month; Patch = None; Build = None }
            }
            
        let calendarVersionPatch =
            gen {
                let! shortCalendarVersion = calendarVersionShort
                let! patch = validPatchUInt32
                return { shortCalendarVersion with Patch = Some patch }
            }
            
        let calendarVersionShortBetaBuild =
            gen {
                let! shortCalendarVersion = calendarVersionShort
                let! betaBuild = Build.Beta.betaBuild
                return { shortCalendarVersion with Build = Some betaBuild }
            }
            
        let calendarVersionPatchBetaBuild =
            gen {
                let! patchCalendarVersion = calendarVersionPatch
                let! betaBuild = Build.Beta.betaBuild
                return { patchCalendarVersion with Build = Some betaBuild }
            }
            
        let calendarVersionShortNightlyBuild =
            gen {
                let! shortCalendarVersion = calendarVersionShort
                let! nightlyBuild = Build.Nightly.nightlyBuild
                return { shortCalendarVersion with Build = Some nightlyBuild }
            }
            
        let calendarVersionPatchNightlyBuild =
            gen {
                let! patchCalendarVersion = calendarVersionPatch
                let! nightlyBuild = Build.Nightly.nightlyBuild
                return { patchCalendarVersion with Build = Some nightlyBuild }
            }
            
        let calendarVersionShortBetaNightlyBuild =
            gen {
                let! shortCalendarVersion = calendarVersionShort
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyBuild
                return { shortCalendarVersion with Build = Some betaNightlyBuild }
            }
            
        let calendarVersionPatchBetaNightlyBuild =
            gen {
                let! patchCalendarVersion = calendarVersionPatch
                let! betaNightlyBuild = Build.BetaNightly.betaNightlyBuild
                return { patchCalendarVersion with Build = Some betaNightlyBuild }
            }
            
        let calendarVersion =
            gen {
                let! choice = Gen.frequency [
                    1, calendarVersionShort
                    1, calendarVersionPatch
                    1, calendarVersionShortNightlyBuild
                    1, calendarVersionPatchNightlyBuild
                    1, calendarVersionShortBetaBuild
                    1, calendarVersionPatchBetaBuild
                ]
                return choice
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
            
        let calendarVersionTagStr =
            gen {
                let! versionPrefix = genTagVersionPrefix
                let! choice = Gen.frequency [
                    1, calendarVersionShortStr
                    1, calendarVersionPatchStr
                    //1, calendarVersionShortNightlyBuildStr
                    //1, calendarVersionPatchNightlyBuildStr                    
                ]
                return $"{versionPrefix}{choice}"
            }
            
        let calendarVersionShortTagStr =
            gen {
                let! versionPrefix = genTagVersionPrefix
                let! calendarVersionShortStr = calendarVersionShortStr
                return $"{versionPrefix}{calendarVersionShortStr}"
            }
            
        let whiteSpaceLeadingTrailingCalendarVersionStr =
            gen {
                let! calVerStr = calendarVersionStr
                
                let! whiteSpacesPrefix = genWhiteSpacesString
                let! whiteSpacesSuffix = genWhiteSpacesString
                
                return $"{whiteSpacesPrefix}{calVerStr}{whiteSpacesSuffix}";
            }
            
        let whiteSpaceLeadingTrailingCalendarVersionTagStr =
            gen {
                let! calVerStr = calendarVersionTagStr
                
                let! whiteSpacesPrefix = genWhiteSpacesString
                let! whiteSpacesSuffix = genWhiteSpacesString
                
                return $"{whiteSpacesPrefix}{calVerStr}{whiteSpacesSuffix}";
            }
            
    module SematicVersion =
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
            
    module DateSteward =
        let inRangeDateTimeOffset =
            gen {
                let min = System.DateTimeOffset(
                    int Calaf.Domain.Year.LowerYearBoundary,
                    int Calaf.Domain.Month.LowerMonthBoundary,
                    int Calaf.Domain.Day.LowerDayBoundary, 0, 0, 0, System.TimeSpan.Zero)
                let max = System.DateTimeOffset(
                    int Calaf.Domain.Year.UpperYearBoundary,
                    int Calaf.Domain.Month.UpperMonthBoundary,
                    int Calaf.Domain.Day.UpperDayBoundary, 23, 59, 59, 999, System.TimeSpan.Zero)
                let dateTimeOffset = Bogus.Faker().Date.BetweenOffset(min, max)
                return dateTimeOffset
            }
            
        let outOfRangeDateTimeOffset =
            gen {
                let min = System.DateTimeOffset(
                    1,
                    int Calaf.Domain.Month.LowerMonthBoundary,
                    int Calaf.Domain.Day.LowerDayBoundary, 0, 0, 0, System.TimeSpan.Zero)
                let max = System.DateTimeOffset(
                    int Calaf.Domain.Year.LowerYearBoundary - 1,
                    int Calaf.Domain.Month.UpperMonthBoundary,
                    int Calaf.Domain.Day.UpperDayBoundary, 23, 59, 59, 999, System.TimeSpan.Zero)
                let dateTimeOffset = Bogus.Faker().Date.BetweenOffset(min, max)
                return dateTimeOffset
            }
        
    module Git =
        open FsCheck
        
        open Calaf.Contracts
        open Calaf.Domain.DomainTypes.Entities
        
        let branchName =            
            Gen.frequency [ 1, SematicVersion.semanticVersionTagStr
                            1, Gen.elements [ "master"; "main"; "develop"; "feature"; "bugfix"; "release" ]]            
        
        let branchNameOrNone =
            Gen.frequency [ 1, Gen.constant None
                            3, branchName |> Gen.map Some ]
            
            
        let commitMessage =
            Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                            1, Gen.elements [""; " "]]
        
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
                let name = commit.Message 
                let! commit = Gen.frequency [
                    1, Gen.constant None
                    3, Gen.constant (Some commit)
                ]
                return { Name = name; Commit = commit }
            }
            
            
        let commit: Gen<Commit> =
            gen {
                let! commitMessage = commitMessage
                let! commitHash = commitHash
                let! timeStamp = genValidDateTimeOffset
                return { Message = commitMessage; Hash = commitHash; When = timeStamp }
            }
            
        let calVerGitTagInfo =
            gen {
                let! validCalVerString = CalendarVersion.calendarVersionTagStr
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
                    3, CalendarVersion.calendarVersionTagStr
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
                3, commit |> Gen.map Some
            ]
            
        let calendarVersionTag : Gen<Tag> =            
            gen {
                // TODO: Rewrite generator!
                let! calendarVersion = CalendarVersion.calendarVersion
                let tagNameSb = System.Text.StringBuilder($"{calendarVersion.Year}.{calendarVersion.Month}")
                let tagNameSb = if calendarVersion.Patch.IsSome then tagNameSb.Append calendarVersion.Patch.Value else tagNameSb 
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
                let! damaged = genBool
                let! unborn = genBool
                let! detached = genBool
                let! branch = branchNameOrNone
                let! commit = Gen.frequency [ 1, Gen.constant None; 3, gitCommitInfo |> Gen.map Some ]
                let! signature = Gen.frequency [1, Gen.constant None; 3, gitSignatureInfo |> Gen.map Some ]
                let! dirty = genBool
                let! tags = Gen.listOf gitTagInfo
                return {
                    Directory = dir
                    Damaged = damaged
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