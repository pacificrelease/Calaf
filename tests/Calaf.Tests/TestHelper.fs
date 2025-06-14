namespace Calaf.Tests

open FsCheck
open FsCheck.FSharp

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities

type MonthStampIncrement = Year | Month | Both

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
        
    let genCommitHash =     
        Gen.constant <| Bogus.Faker().Random.Hash()
        
    let genHexadecimal =
        Gen.constant <| Bogus.Faker().Random.Hexadecimal()
        
    let genFrom1To512LettersString =
        Gen.constant <| Bogus.Faker().Random.String2(1, 512)
        
    let genByte =
        Gen.choose(int 0uy, int 255uy) |> Gen.map byte
        
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
        
    module Build =
        let hashString =
            gen {
                let! hash = Gen.frequency [
                    1, genCommitHash
                    1, genHexadecimal
                    1, genFrom1To512LettersString
                ]
                return hash
            }
            
        let nightlyString =
            gen {
                let! nightly = Gen.elements ["nightly"; "NIGHTLY"; "Nightly"; "NiGhTlY"; "nIgHtLy"; "NIGHTly"; "nightLY"]
                let! day = genDay
                let! number = genByte
                let! hash = hashString
                return $"{nightly}{Calaf.Domain.Build.BuildTypeDayDivider}{day}{Calaf.Domain.Build.DayNumberDivider}{number:D2}{Calaf.Domain.Build.NumberHashDivider}{hash}"
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
            
        let containingNightlyBadString =
            gen {
                let! nightlyString = nightlyString
                let! wrongString = wrongString
                let! leadingWhiteSpaces = genWhiteSpacesString
                let! choice = Gen.frequency [
                    1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                    1, Gen.constant $"{Calaf.Domain.Build.NumberHashDivider}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{Calaf.Domain.Build.NumberHashDivider}"
                    1, Gen.constant $"{nightlyString}{wrongString}{nightlyString}"
                    1, Gen.constant $"{wrongString}{nightlyString}{wrongString}"
                    1, Gen.constant $"{wrongString}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{wrongString}"
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
                let! day = genDay
                let! number = genByte
                let! hash = Gen.frequency [
                    1, Gen.constant None
                    1, hashString |> Gen.map Some
                ] 
                return { Day = day; Number = number; Hash = hash } |> Build.Nightly
            }
            
        
    module Month =
        let inRangeByteMonth =
            gen {
                let! month = Gen.choose(int Calaf.Domain.Month.lowerMonthBoundary,
                                        int Calaf.Domain.Month.upperMonthBoundary)
                return byte month
            }
            
        let outOfRangeByteMonth =            
            gen {
                let! month = Gen.choose(int Calaf.Domain.Month.upperMonthBoundary + 1, int System.Byte.MaxValue)
                return byte month 
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
                    Gen.choose(int Calaf.Domain.Year.lowerYearBoundary,
                               int Calaf.Domain.Year.upperYearBoundary)
                return uint16 year
            }
            
        let outOfRangeUInt16Year =
            let outOfRangeCornerCases = Gen.elements [ 0;
                                                       int Calaf.Domain.Year.lowerYearBoundary - 1
                                                       int Calaf.Domain.Year.upperYearBoundary + 1 ]
            let outOfRangeLowerThaAllowed = Gen.choose(int System.UInt16.MinValue, int Calaf.Domain.Year.lowerYearBoundary - 1)
            let outOfRangeGreaterThaAllowed = Gen.choose(int Calaf.Domain.Year.upperYearBoundary + 1, int System.UInt16.MaxValue)
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
                let! lowerThanAllowed   = Gen.choose(1, int Calaf.Domain.Year.lowerYearBoundary - 1)
                let! greaterThanAllowed = Gen.choose(int Calaf.Domain.Year.upperYearBoundary + 1, int System.UInt16.MaxValue)
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
            
    module internal CalendarVersion =
        let calendarVersionShortStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                return $"{year}.{month}"
            }
            
        let calendarVersionPatchStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                return $"{year}.{month}.{patch}"
            }
            
        let calendarVersionShortNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! nightlyBuild = Build.nightlyString
                return $"{year}.{month}-{nightlyBuild}"                 
            }
            
        let calendarVersionPatchNightlyBuildStr =
            gen {
                let! year  = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! patch = validPatchUInt32
                let! nightlyBuild = Build.nightlyString
                return $"{year}.{month}.{patch}-{nightlyBuild}"                 
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
            
        let calendarVersionShortNightlyBuild =
            gen {
                let! shortCalendarVersion = calendarVersionShort
                let! nightlyBuild = Build.nightlyBuild
                return { shortCalendarVersion with Build = Some nightlyBuild }
            }
            
        let calendarVersionPatchNightlyBuild =
            gen {
                let! patchCalendarVersion = calendarVersionPatch
                let! nightlyBuild = Build.nightlyBuild
                return { patchCalendarVersion with Build = Some nightlyBuild }
            }
            
        let calendarVersion =
            gen {
                let! choice = Gen.frequency [
                    1, calendarVersionShort
                    1, calendarVersionPatch
                    1, calendarVersionShortNightlyBuild
                    1, calendarVersionPatchNightlyBuild
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
        
        let calendarVersionShortMonthStamp =
            let genCalVer =
                gen { 
                    let! year  = Gen.choose(int Calaf.Domain.Year.lowerYearBoundary, int Calaf.Domain.Year.upperYearBoundary - 1)
                    let! month = Gen.choose(1, 11)       
                    return { Year = uint16 year; Month = byte month; Patch = None; Build = None }
                }            
            gen {
                let! calVer = genCalVer
                let dateStamp = { Year = calVer.Year; Month = calVer.Month }
                return (calVer, dateStamp)
            }
            
        let calendarVersionPatchMonthStamp =            
            gen {
                let! calVer, dateStamp = calendarVersionShortMonthStamp
                let! patch = validPatchUInt32 
                return ({ calVer with Patch = Some patch }, dateStamp)
            }
            
        let calendarVersionMonthStamp =
            gen {
                let! choice =  Gen.frequency [
                    1, calendarVersionShortMonthStamp
                    1, calendarVersionPatchMonthStamp
                ]
                return choice  
            }        
            
    module internal SematicVersion =
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
            
    let invalidThreePartString =
        gen {
            let! first  = nonNumericString
            let! second = nonNumericString
            let! third  = nonNumericString
            return $"{first}.{second}.{third}"
        }
            
    module internal DateSteward =
        let inRangeDateTime =
            gen {
                let! year = Year.inRangeUInt16Year
                let! month = Month.inRangeByteMonth
                let! day = Gen.choose(1, 28)
                return (int year, int month, day) |> System.DateTime
            }
            
        let outOfRangeDateTime =
            gen {
                let! outOfRangeLowerThaAllowed = Gen.choose(int System.UInt16.MinValue + 1, int Calaf.Domain.Year.lowerYearBoundary - 1)
                let! month = Month.inRangeByteMonth
                let! day = Gen.choose(1, 28)
                return (outOfRangeLowerThaAllowed, int month, day) |> System.DateTime
            }    
        
    let monthStampIncrement =
        gen {
            return! Gen.elements [ MonthStampIncrement.Year; MonthStampIncrement.Month; MonthStampIncrement.Both ]
        }
        
    module Git =
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
            
    type internal invalidThreePartString =
        static member invalidThreePartString() =
            Arb.fromGen Generator.invalidThreePartString    
            
    type internal monthStampIncrement =
        static member monthStampIncrement() =
            Arb.fromGen Generator.monthStampIncrement
            
    type internal directoryPathString =
        static member directoryPathString() =
            Arb.fromGen Generator.directoryPathString
            
    module internal Build =
        type nightlyString =
            static member nightlyString() =
                Arb.fromGen Generator.Build.nightlyString
                
        type wrongString =
            static member wrongString() =
                Arb.fromGen Generator.Build.wrongString
                
        type containingNightlyBadString =
            static member containingNightlyBadString() =
                Arb.fromGen Generator.Build.containingNightlyBadString
                
        type nightlyBuild =
            static member nightlyBuild() =
                Arb.fromGen Generator.Build.nightlyBuild
            
    module internal Month =
        type inRangeByteMonth =
            static member inRangeByteMonth() =
                Arb.fromGen Generator.Month.inRangeByteMonth
                
        type outOfRangeUInt16Year =
            static member outOfRangeByteMonth() =
                Arb.fromGen Generator.Month.outOfRangeByteMonth
                
        type outOfRangeStringMonth =
            static member outOfRangeStringMonth() =
                Arb.fromGen (Generator.Month.outOfRangeByteMonth |> Gen.map string)
                
        type leadingZeroOutOfRangeStringMonth =
            static member leadingZeroOutOfRangeStringMonth() =
                Arb.fromGen Generator.Month.leadingZeroOutOfRangeStringMonth
                
        type wrongInt32Month =
            static member wrongInt32Month() =
                Arb.fromGen Generator.Month.wrongInt32Month
                
        type wrongStringMonth =
            static member wrongStringMonth() =
                Arb.fromGen Generator.Month.wrongStringMonth
        
        
    module internal Year =
        type inRangeUInt16Year =
            static member inRangeUInt16Year() =
                Arb.fromGen Generator.Year.inRangeUInt16Year
                
        type outOfRangeUInt16Year =
            static member outOfRangeUInt16Year() =
                Arb.fromGen Generator.Year.outOfRangeUInt16Year
                
        type outOfRangeStringYear =
            static member outOfRangeUInt16Year() =
                Arb.fromGen (Generator.Year.outOfRangeUInt16Year |> Gen.map string)
                
        type leadingZeroOutOfRangeStringYear =
            static member leadingZeroOutOfRangeStringYear() =
                Arb.fromGen Generator.Year.leadingZeroOutOfRangeStringYear
                
        type wrongInt32Year =
            static member wrongInt32Year() =
                Arb.fromGen Generator.Year.wrongInt32Year
                
        type wrongStringYear =
            static member wrongStringYear() =
                Arb.fromGen Generator.Year.wrongStringYear
                
    module internal CalendarVersion =
        type internal calendarVersionPatchStr =
            static member calendarVersionPatchStr() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionPatchStr
                
        type internal calendarVersionShortStr =
            static member calendarVersionShortStr() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortStr
                
        type internal calendarVersion =
            static member calendarVersion() =
                Arb.fromGen Generator.CalendarVersion.calendarVersion

        type internal calendarVersionShort =
            static member calendarVersionShort() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShort
                
        type internal calendarVersionPatch =
            static member calendarVersionPatch() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionPatch
                
        type internal calendarVersionShortNightlyBuild =
            static member calendarVersionShortNightlyBuild() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortNightlyBuild 
                
        type internal calendarVersions =
            static member calendarVersions() =
                Arb.fromGen Generator.CalendarVersion.calendarVersions
        
        type internal calendarVersionShortMonthStamp =
            static member calendarVersionShortMonthStamp() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortMonthStamp
                
        type internal calendarVersionPatchMonthStamp =
            static member calendarVersionPatchMonthStamp() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionPatchMonthStamp
                
        type internal calendarVersionMonthStamp =
            static member calendarVersionMonthStamp() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionMonthStamp
                
        type internal calendarVersionTagStr =
            static member calendarVersionTagStr() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionTagStr
                
        type internal calendarVersionShortTagStr =
            static member calendarVersionShortTagStr() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortTagStr
        
        type internal whiteSpaceLeadingTrailingCalendarVersionStr =
            static member whiteSpaceLeadingTrailingCalendarVersionStr() =
                Arb.fromGen Generator.CalendarVersion.whiteSpaceLeadingTrailingCalendarVersionStr
                
        type internal whiteSpaceLeadingTrailingCalendarVersionTagStr =
            static member whiteSpaceLeadingTrailingCalendarVersionTagStr() =
                Arb.fromGen Generator.CalendarVersion.whiteSpaceLeadingTrailingCalendarVersionTagStr
        
                
    module internal SematicVersion =
        type internal semanticVersionStr =
            static member semanticVersionStr() =
                Arb.fromGen Generator.SematicVersion.semanticVersionStr
                
    module internal DateSteward =
        type inRangeDateTime =
            static member inRangeDateTime() =
                Arb.fromGen Generator.DateSteward.inRangeDateTime
                
        type outOfRangeDateTime =
            static member outOfRangeDateTime() =
                Arb.fromGen Generator.DateSteward.outOfRangeDateTime
    
    module internal Git =
        type branchName =
            static member branchName() =
                Arb.fromGen Generator.Git.branchName
                
        type gitCommitInfo =
            static member gitCommitInfo() =
                Arb.fromGen Generator.Git.gitCommitInfo
                
        type calVerGitTagInfo =
            static member calVerGitTagInfo() =
                Arb.fromGen Generator.Git.calVerGitTagInfo
                
        type semVerGitTagInfo =
            static member semVerGitTagInfo() =
                Arb.fromGen Generator.Git.semVerGitTagInfo
                
        type malformedGitTagInfo =
            static member malformedGitTagInfo() =
                Arb.fromGen Generator.Git.malformedGitTagInfo
                
        type calendarVersionOrSemanticVersionGitTagInfo =
            static member calendarVersionOrSemanticVersionGitTagInfo() =
                Arb.fromGen Generator.Git.calendarVersionOrSemanticVersionGitTagInfo
                
        type randomGitTagInfo =
            static member randomGitTagInfo() =
                Arb.fromGen Generator.Git.randomGitTagInfo
                
        type calendarVersionsTagsArray =
             static member calendarVersionsTagsArray() =
                Arb.fromGen Generator.Git.calendarVersionsTagsArray
                
        type semanticVersionsTagsArray =
            static member semanticVersionsTagsArray() =
                Arb.fromGen Generator.Git.semanticVersionsTagsArray
                
        type unversionedTagsArray =
            static member unversionedTagsArray() =
                Arb.fromGen Generator.Git.unversionedTagsArray
                
        type semanticVersionsAndUnversionedTagsArray =
            static member semanticVersionsAndUnversionedTagsArray() =
                Arb.fromGen Generator.Git.semanticVersionsAndUnversionedTagsArray
                
        type baseGitRepositoryInfo =
            static member baseGitRepositoryInfo() =
                Arb.fromGen Generator.Git.baseGitRepositoryInfo