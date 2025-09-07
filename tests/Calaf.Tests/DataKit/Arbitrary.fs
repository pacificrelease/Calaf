namespace Calaf.Tests

open FsCheck.FSharp

module Arbitrary =
    type internal validMicroUInt32 =
        static member validMicroUInt32() =
            Arb.fromGen Generator.Common.validMicroUInt32
            
    type internal greaterThanZeroBeforeUInt32MinusOne =
        static member greaterThanZeroUInt32() =
            Arb.fromGen Generator.Common.greaterThanZeroBeforeUInt32MinusOne
            
    type internal nonNumericString =
        static member nonNumericString() =
            Arb.fromGen Generator.Common.nonNumericString
            
    type internal nullOrWhiteSpaceString =
        static member nullOrWhiteSpaceString() =
            Arb.fromGen Generator.Common.nullOrWhiteSpaceString
            
    type internal overflowMicroString =
        static member overflowMicroString() =
            Arb.fromGen Generator.Common.overflowMicroString
            
    type internal invalidThreePartString =
        static member invalidThreePartString() =
            Arb.fromGen Generator.Common.invalidThreePartString 
            
    type internal directoryPathString =
        static member directoryPathString() =
            Arb.fromGen Generator.Common.directoryPathString
            
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
                    
        module ReleaseCandidate =            
            type rcString =
                static member rcString() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.rcString
                    
            type containingReleaseCandidateBadString =
                static member containingReleaseCandidateBadString() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.containingReleaseCandidateBadString
            
            type rc =
                static member rc() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.rc
                    
            type rcBuild =
                static member rcBuild() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.rcBuild
                    
            type rcBuildOption =
                static member rcBuildOption() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.rcBuildOption
                    
            type numberNoUpperBoundaryReleaseCandidateBuild =
                static member numberNoUpperBoundaryReleaseCandidateBuild() =
                    Arb.fromGen Generator.Build.ReleaseCandidate.numberNoUpperBoundaryReleaseCandidateBuild
                    
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
                    
        module ReleaseCandidateNightly =
            type rcNightlyString =
                static member rcNightlyString() =
                    Arb.fromGen Generator.Build.ReleaseCandidateNightly.rcNightlyString
                    
            type containingReleaseCandidateNightlyBadString =
                static member containingReleaseCandidateNightlyBadString() =
                    Arb.fromGen Generator.Build.ReleaseCandidateNightly.containingReleaseCandidateNightlyBadString
                
            type rcNightlyBuild =
                static member rcNightlyBuild() =
                    Arb.fromGen Generator.Build.ReleaseCandidateNightly.rcNightlyBuild
                    
            type rcNightlyBuildOption =
                static member rcNightlyBuildOption() =
                    Arb.fromGen Generator.Build.ReleaseCandidateNightly.rcNightlyBuildOption
                
            type numberNoUpperBoundaryReleaseCandidateNightlyBuild =
                static member numberNoUpperBoundaryReleaseCandidateNightlyBuild() =
                    Arb.fromGen Generator.Build.ReleaseCandidateNightly.numberNoUpperBoundaryReleaseCandidateNightlyBuild 
                
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.Stable.Micro
                    
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.Alpha.Micro
                    
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.Beta.Micro
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.Beta.Accidental
                    
        module ReleaseCandidate =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidate.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidate.TagString
                    
            type internal Short =
                static member calendarVersionShort() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidate.Short
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidate.Micro
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidate.Accidental
            
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.Nightly.Micro
                    
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.AlphaNightly.Micro
                    
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
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.BetaNightly.Micro
                    
            type internal Accidental =
                static member Accidental() =
                        Arb.fromGen Generator.CalendarVersion.BetaNightly.Accidental
                        
        module ReleaseCandidateNightly =
            type internal String =
                static member String() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidateNightly.String
                    
            type internal TagString =
                static member TagString() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidateNightly.TagString
                
            type internal Short =
                static member Short() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidateNightly.Short
                    
            type internal Micro =
                static member Micro() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidateNightly.Micro
                    
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.CalendarVersion.ReleaseCandidateNightly.Accidental
        
        type internal ShortString =
            static member ShortString() =
                Arb.fromGen Generator.CalendarVersion.ShortString
                
        type internal MicroString =
            static member MicroString() =
                Arb.fromGen Generator.CalendarVersion.MicroString
                
        type internal String =
            static member String() =
                Arb.fromGen Generator.CalendarVersion.String
        
        type internal ShortTagString =
            static member ShortTagString() =
                Arb.fromGen Generator.CalendarVersion.ShortTagString
        
        type internal MicroTagString =
            static member MicroTagString() =
                Arb.fromGen Generator.CalendarVersion.MicroTagString
        
        type internal TagStrictString =
            static member TagStrictString() =
                Arb.fromGen Generator.CalendarVersion.TagStrictString
                
        type internal TagString =
            static member TagString() =
                Arb.fromGen Generator.CalendarVersion.TagString
        
        type internal AccidentalShort =
            static member AccidentalShort() =
                Arb.fromGen Generator.CalendarVersion.AccidentalShort
                
        type internal AccidentalMicro =
            static member AccidentalMicro() =
                Arb.fromGen Generator.CalendarVersion.AccidentalMicro
                
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
        
        module CommitMessage =
            type internal FeatNonBreakingChangeString =
                static member FeatNonBreakingChangeString() =
                    Arb.fromGen Generator.Git.CommitMessage.FeatNonBreakingChangeString
                    
            type internal FeatBreakingChangeString =
                static member FeatBreakingChangeString() =
                    Arb.fromGen Generator.Git.CommitMessage.FeatBreakingChangeString
                    
            type internal FixNonBreakingChangeString =
                static member FixNonBreakingChangeString() =
                    Arb.fromGen Generator.Git.CommitMessage.FixNonBreakingChangeString
                    
            type internal FixBreakingChangeString =
                static member FixBreakingChangeString() =
                    Arb.fromGen Generator.Git.CommitMessage.FixBreakingChangeString
                    
            type internal OtherString =
                static member OtherString() =
                    Arb.fromGen Generator.Git.CommitMessage.OtherString
                    
            type internal EmptyString =
                static member EmptyString() =
                    Arb.fromGen Generator.Git.CommitMessage.EmptyString
                    
            type internal AccidentalString =
                static member AccidentalString() =
                    Arb.fromGen Generator.Git.CommitMessage.AccidentalString
                    
            type internal FeatNonBreakingChange =
                static member FeatNonBreakingChange() =
                    Arb.fromGen Generator.Git.CommitMessage.FeatNonBreakingChange
                    
            type internal FeatBreakingChange =
                static member FeatBreakingChange() =
                    Arb.fromGen Generator.Git.CommitMessage.FeatBreakingChange
                    
            type internal FixNonBreakingChange =
                static member FixNonBreakingChange() =
                    Arb.fromGen Generator.Git.CommitMessage.FixNonBreakingChange
                    
            type internal FixBreakingChange =
                static member FixBreakingChange() =
                    Arb.fromGen Generator.Git.CommitMessage.FixBreakingChange
                    
            type internal Other =
                static member Other() =
                    Arb.fromGen Generator.Git.CommitMessage.Other
                    
            type internal Empty =
                static member Empty() =
                    Arb.fromGen Generator.Git.CommitMessage.Empty
            
        module Commit =
            type internal Accidental =
                static member Accidental() =
                    Arb.fromGen Generator.Git.Commit.Accidental
                    
            type internal FeatList =
                static member FeatList() =
                    Arb.fromGen Generator.Git.Commit.FeatList
                    
            type internal FixList =
                static member FixList() =
                    Arb.fromGen Generator.Git.Commit.FixList
                    
            type internal OtherList =
                static member OtherList() =
                    Arb.fromGen Generator.Git.Commit.OtherList
                    
            type internal AccidentalsList =
                static member AccidentalsList() =
                    Arb.fromGen Generator.Git.Commit.AccidentalsList
                    
        module Changeset =
            type internal Features =
                static member Features() =
                    Arb.fromGen Generator.Git.Changeset.Features
                    
            type internal FeaturesChangeset =
                static member FeaturesChangeset() =
                    Arb.fromGen Generator.Git.Changeset.FeaturesChangeset