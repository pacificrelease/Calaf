namespace Calaf.Tests

open FsCheck.FSharp

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
            
    type internal directoryPathString =
        static member directoryPathString() =
            Arb.fromGen Generator.directoryPathString
            
    module Build =
        type wrongString =
            static member wrongString() =
                Arb.fromGen Generator.Build.wrongString
        
        module Alpha =            
            type alphaString =
                static member alphaString() =
                    Arb.fromGen Generator.Build.Alpha.String
                    
            type containingAlphaBadString =
                static member containingBetaBadString() =
                    Arb.fromGen Generator.Build.Alpha.BadString
            
            type alpha =
                static member alpha() =
                    Arb.fromGen Generator.Build.Alpha.Accidental
                    
            type alphaBuild =
                static member alphaBuild() =
                    Arb.fromGen Generator.Build.Alpha.AccidentalBuild
                    
            type alphaBuildOption =
                static member alphaBuildOption() =
                    Arb.fromGen Generator.Build.Alpha.AccidentalBuildOption
                    
            type numberNoUpperBoundaryAlphaBuild =
                static member numberNoUpperBoundaryAlphaBuild() =
                    Arb.fromGen Generator.Build.Alpha.NoUpperBoundaryAccidentalBuild
                    
        module Beta =            
            type betaString =
                static member betaString() =
                    Arb.fromGen Generator.Build.Beta.betaString
                    
            type containingBetaBadString =
                static member containingBetaBadString() =
                    Arb.fromGen Generator.Build.Beta.containingBetaBadString
            
            type beta =
                static member beta() =
                    Arb.fromGen Generator.Build.Beta.beta
                    
            type betaBuild =
                static member betaBuild() =
                    Arb.fromGen Generator.Build.Beta.betaBuild
                    
            type betaBuildOption =
                static member betaBuildOption() =
                    Arb.fromGen Generator.Build.Beta.betaBuildOption
                    
            type numberNoUpperBoundaryBetaBuild =
                static member numberNoUpperBoundaryBetaBuild() =
                    Arb.fromGen Generator.Build.Beta.numberNoUpperBoundaryBetaBuild
                    
        module Nightly =
            type nightlyString =
                static member nightlyString() =
                    Arb.fromGen Generator.Build.Nightly.nightlyString
                    
            type containingNightlyBadString =
                static member containingNightlyBadString() =
                    Arb.fromGen Generator.Build.Nightly.containingNightlyBadString
            
            type nightly =
                static member nightly() =
                    Arb.fromGen Generator.Build.Nightly.nightly
                    
            type nightlyBuild =
                static member nightlyBuild() =
                    Arb.fromGen Generator.Build.Nightly.nightlyBuild
                    
            type nightlyBuildOption =
                static member nightlyBuildOption() =
                    Arb.fromGen Generator.Build.Nightly.nightlyBuildOption
                    
            type numberNoUpperBoundaryNightlyBuild =
                static member numberNoUpperBoundaryNightlyBuild() =
                    Arb.fromGen Generator.Build.Nightly.numberNoUpperBoundaryNightlyBuild
                    
        module AlphaNightly =
            type alphaNightlyString =
                static member alphaNightlyString() =
                    Arb.fromGen Generator.Build.AlphaNightly.String
                    
            type containingAlphaNightlyBadString =
                static member containingAlphaNightlyBadString() =
                    Arb.fromGen Generator.Build.AlphaNightly.BadString
                
            type alphaNightlyBuild =
                static member alphaNightlyBuild() =
                    Arb.fromGen Generator.Build.AlphaNightly.AccidentalBuild
                    
            type alphaNightlyBuildOption =
                static member alphaNightlyBuildOption() =
                    Arb.fromGen Generator.Build.AlphaNightly.AccidentalBuildOption
                
            type numberNoUpperBoundaryAlphaNightlyBuild =
                static member numberNoUpperBoundaryAlphaNightlyBuild() =
                    Arb.fromGen Generator.Build.AlphaNightly.NoUpperBoundaryAccidentalBuild
                    
        module BetaNightly =
            type betaNightlyString =
                static member betaNightlyString() =
                    Arb.fromGen Generator.Build.BetaNightly.betaNightlyString
                    
            type containingBetaNightlyBadString =
                static member containingBetaNightlyBadString() =
                    Arb.fromGen Generator.Build.BetaNightly.containingBetaNightlyBadString
                
            type betaNightlyBuild =
                static member betaNightlyBuild() =
                    Arb.fromGen Generator.Build.BetaNightly.betaNightlyBuild
                    
            type betaNightlyBuildOption =
                static member betaNightlyBuildOption() =
                    Arb.fromGen Generator.Build.BetaNightly.betaNightlyBuildOption
                
            type numberNoUpperBoundaryBetaNightlyBuild =
                static member numberNoUpperBoundaryBetaNightlyBuild() =
                    Arb.fromGen Generator.Build.BetaNightly.numberNoUpperBoundaryBetaNightlyBuild 
                
    module internal Day =
        type inRangeByteDay =
            static member inRangeByteDay() =
                Arb.fromGen Generator.Day.inRangeByteDay
                
        type outOfRangeByteDay =
            static member outOfRangeByteDay() =
                Arb.fromGen Generator.Day.outOfRangeByteDay
                
        type wrongInt32Day =
            static member wrongInt32Day() =
                Arb.fromGen Generator.Day.wrongInt32Day
            
    module internal Month =
        type inRangeByteMonth =
            static member inRangeByteMonth() =
                Arb.fromGen Generator.Month.inRangeByteMonth
                
        type outOfRangeByteMonth =
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
        module Stable =                    
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.Stable.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.Stable.TagString
                    
            type internal Short =
                static member calendarVersionShort() =
                    Arb.fromGen Generator.CalendarVersion.Stable.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.Stable.Patch
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.Stable.Accidental
        
        module Alpha =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.TagString
                    
            type internal Short =
                static member calendarVersionShort() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.Patch
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.Accidental
                    
        module Beta =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.Beta.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.Beta.TagString
                    
            type internal Short =
                static member calendarVersionShort() =
                    Arb.fromGen Generator.CalendarVersion.Beta.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.Beta.Patch
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.Beta.Accidental
            
        module Nightly =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.TagString
                
            type internal Short =
                static member Short() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.Patch
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.Accidental
                    
        module AlphaNightly =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.AlphaNightly.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.AlphaNightly.TagString
                
            type internal Short =
                static member Short() =
                    Arb.fromGen Generator.CalendarVersion.AlphaNightly.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.AlphaNightly.Patch
                    
            type internal Accidental =
                static member Accidental() =
                        Arb.fromGen Generator.CalendarVersion.AlphaNightly.Accidental
                        
        module BetaNightly =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.BetaNightly.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.BetaNightly.TagString
                
            type internal Short =
                static member Short() =
                    Arb.fromGen Generator.CalendarVersion.BetaNightly.Short
                    
            type internal Patch =
                static member Patch() =
                    Arb.fromGen Generator.CalendarVersion.BetaNightly.Patch
                    
            type internal Accidental =
                static member Accidental() =
                        Arb.fromGen Generator.CalendarVersion.BetaNightly.Accidental
        
        type internal ShortString =
            static member ShortString() =
                Arb.fromGen Generator.CalendarVersion.ShortString
                
        type internal PatchString =
            static member PatchString() =
                Arb.fromGen Generator.CalendarVersion.PatchString
                
        type internal String =
            static member String() =
                Arb.fromGen Generator.CalendarVersion.String
        
        type internal ShortTagString =
            static member ShortTagString() =
                Arb.fromGen Generator.CalendarVersion.ShortTagString
        
        type internal PatchTagString =
            static member PatchTagString() =
                Arb.fromGen Generator.CalendarVersion.PatchTagString
        
        type internal TagStrictString =
            static member TagStrictString() =
                Arb.fromGen Generator.CalendarVersion.TagStrictString
                
        type internal TagString =
            static member TagString() =
                Arb.fromGen Generator.CalendarVersion.TagString
        
        type internal AccidentalShort =
            static member AccidentalShort() =
                Arb.fromGen Generator.CalendarVersion.AccidentalShort
                
        type internal AccidentalPatch =
            static member AccidentalPatch() =
                Arb.fromGen Generator.CalendarVersion.AccidentalPatch
                
        type AccidentalPreReleases =
            static member AccidentalPreReleases() =
                Arb.fromGen Generator.CalendarVersion.AccidentalPreReleases
                
        type internal Accidental =
            static member Accidental() =
                Arb.fromGen Generator.CalendarVersion.Accidental
        
        type internal AccidentalsArray =
            static member AccidentalsArray() =
                Arb.fromGen Generator.CalendarVersion.AccidentalsArray
        
        type internal WhiteSpaceLeadingTrailingString =
            static member WhiteSpaceLeadingTrailingString() =
                Arb.fromGen Generator.CalendarVersion.WhiteSpaceLeadingTrailingString
                
        type internal WhiteSpaceLeadingTrailingTagString =
            static member WhiteSpaceLeadingTrailingTagString() =
                Arb.fromGen Generator.CalendarVersion.WhiteSpaceLeadingTrailingTagString
                
    module internal SematicVersion =
        type internal semanticVersionStr =
            static member semanticVersionStr() =
                Arb.fromGen Generator.SematicVersion.semanticVersionStr
                
    module internal DateSteward =
        type inRangeDateTimeOffset =
            static member inRangeDateTimeOffset() =
                Arb.fromGen Generator.DateSteward.inRangeDateTimeOffset
                
        type outOfRangeDateTimeOffset =
            static member outOfRangeDateTimeOffset() =
                Arb.fromGen Generator.DateSteward.outOfRangeDateTimeOffset
    
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