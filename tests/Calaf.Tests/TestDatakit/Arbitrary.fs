﻿namespace Calaf.Tests

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
                
        type betaBuild =
            static member betaBuild() =
                Arb.fromGen Generator.Build.betaBuild
                
                
        type nightlyBuild =
            static member nightlyBuild() =
                Arb.fromGen Generator.Build.nightlyBuild
                
        type numberNoUpperBoundaryNightlyBuild =
            static member numberNoUpperBoundaryNightlyBuild() =
                Arb.fromGen Generator.Build.numberNoUpperBoundaryNightlyBuild
        
        type betaBuildOption =
            static member betaBuildOption() =
                Arb.fromGen Generator.Build.betaBuildOption
                
        type nightlyBuildOption =
            static member nightlyBuildOption() =
                Arb.fromGen Generator.Build.nightlyBuildOption
                
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
        type internal calendarVersionNightlyBuildStr =
            static member calendarVersionNightlyBuildStr() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionNightlyBuildStr
                
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
                
        type internal calendarVersionShortBetaBuild =
            static member calendarVersionShortBetaBuild() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortBetaBuild
                
        type internal calendarVersionShortNightlyBuild =
            static member calendarVersionShortNightlyBuild() =
                Arb.fromGen Generator.CalendarVersion.calendarVersionShortNightlyBuild
                
        type internal calendarVersions =
            static member calendarVersions() =
                Arb.fromGen Generator.CalendarVersion.calendarVersions
                
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