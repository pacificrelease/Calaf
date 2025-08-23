module Calaf.Tests.Generator.CalendarVersion

open FsCheck.FSharp

open Calaf.Domain.DomainTypes.Values
open Calaf.Tests.Generator.Primitive

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