module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal NumberIncrementStep = 1us
[<Literal>]
let internal NumberStartValue = 1us
[<Literal>]
let internal NightlyZeroPrefix = 0
[<Literal>]
let internal NightlyBuildType = "nightly"
[<Literal>]
let internal AlphaBuildType = "alpha"
[<Literal>]
let internal BetaBuildType = "beta"
[<Literal>]
let internal ReleaseCandidateBuildType = "rc"
[<Literal>]
let internal NightlyZeroBuildTypeDivider = "."
[<Literal>]
let internal BuildTypeDayDivider = "."
[<Literal>]
let internal DayNumberDivider = "."
[<Literal>]
let internal BuildTypeNumberDivider = "."
[<Literal>]
let internal PreReleaseNightlyDivider = "."
let private nightlyBuildPattern =        
    $@"{NightlyZeroPrefix}\{NightlyZeroBuildTypeDivider}(?i:({NightlyBuildType}))\{BuildTypeDayDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"    
let private alphaBuildPattern =        
    $@"(?i:({AlphaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})$"
let private alphaNightlyBuildPattern =
    $@"(?i:({AlphaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})\{PreReleaseNightlyDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
let private betaBuildPattern =        
    $@"(?i:({BetaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})$"
let private betaNightlyBuildPattern =        
    $@"(?i:({BetaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})\{PreReleaseNightlyDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
let private releaseCandidateBuildPattern =        
    $@"(?i:({ReleaseCandidateBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})$"
let private releaseCandidateNightlyBuildPattern =        
    $@"(?i:({ReleaseCandidateBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})\{PreReleaseNightlyDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"

let private allowedNightlyBuildRegexString =
    $@"^{nightlyBuildPattern}"
let private allowedAlphaBuildRegexString =
    $@"^{alphaBuildPattern}"
let private allowedAlphaNightlyBuildRegexString =
    $@"^{alphaNightlyBuildPattern}"
let private allowedBetaBuildRegexString =
    $@"^{betaBuildPattern}"
let private allowedBetaNightlyBuildRegexString =
    $@"^{betaNightlyBuildPattern}"
let private allowedReleaseCandidateBuildRegexString =
    $@"^{releaseCandidateBuildPattern}"
let private allowedReleaseCandidateNightlyBuildRegexString =
    $@"^{releaseCandidateNightlyBuildPattern}"
    
let private matchNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedNightlyBuildRegexString)

let private matchAlphaBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedAlphaBuildRegexString)
    
let private matchAlphaNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedAlphaNightlyBuildRegexString)
    
let private matchBetaBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedBetaBuildRegexString)
    
let private matchBetaNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedBetaNightlyBuildRegexString)

let private matchReleaseCandidateBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedReleaseCandidateBuildRegexString)
    
let private matchReleaseCandidateNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedReleaseCandidateNightlyBuildRegexString)

let private nextNumber currentNumber : BuildNumber =
    let isOverflowPossible = currentNumber = BuildNumber.MaxValue
    if isOverflowPossible then
        NumberStartValue
    else currentNumber + NumberIncrementStep

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)

let private (|Alpha|_|) (input: string) =
    let m = matchAlphaBuildRegex input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else None
        
let private (|AlphaNightly|_|) (input: string) =
    let m = matchAlphaNightlyBuildRegex input
    if m.Success then
        let alphaNumberSegment    = m.Groups[2].Value
        let nightlyDaySegment    = m.Groups[3].Value
        let nightlyNumberSegment = m.Groups[4].Value
        Some (alphaNumberSegment, nightlyDaySegment, nightlyNumberSegment)
    else None
    
let private (|Beta|_|) (input: string) =
    let m = matchBetaBuildRegex input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else None
        
let private (|BetaNightly|_|) (input: string) =
    let m = matchBetaNightlyBuildRegex input
    if m.Success then
        let betaNumberSegment    = m.Groups[2].Value
        let nightlyDaySegment    = m.Groups[3].Value
        let nightlyNumberSegment = m.Groups[4].Value
        Some (betaNumberSegment, nightlyDaySegment, nightlyNumberSegment)
    else None
    
let private (|ReleaseCandidate|_|) (input: string) =
    let m = matchReleaseCandidateBuildRegex input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else None
    
let private (|ReleaseCandidateNightly|_|) (input: string) =
    let m = matchReleaseCandidateNightlyBuildRegex input
    if m.Success then
        let rcNumberSegment      = m.Groups[2].Value
        let nightlyDaySegment    = m.Groups[3].Value
        let nightlyNumberSegment = m.Groups[4].Value
        Some (rcNumberSegment, nightlyDaySegment, nightlyNumberSegment)
    else None
    
let private (|Nightly|_|) (input: string) =
    let m = matchNightlyBuildRegex input
    if m.Success then
        let daySegment    = m.Groups[2].Value
        let numberSegment = m.Groups[3].Value
        Some (daySegment, numberSegment)
    else None

let private tryCreateReleaseCandidateNightlyBuild (rcNumberString: string, nightlyDayString: string, nightlyNumberString: string) =
    match (UInt16.TryParse rcNumberString, Byte.TryParse nightlyDayString, UInt16.TryParse nightlyNumberString) with
    | (true, rcNumber), (true, nightlyDay), (true, nightlyNumber) ->
        let rcNightly = Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = nightlyDay; Number = nightlyNumber })
        Ok rcNightly
    | _ -> Error BuildInvalidString
    
let private tryCreateReleaseCandidateBuild (numberString: string) =
    match UInt16.TryParse numberString with
    | true, buildNumber ->
        let rc = Build.ReleaseCandidate { Number = buildNumber }
        Ok rc
    | _ -> Error BuildInvalidString
    
let private tryCreateBetaNightlyBuild (betaNumberString: string, nightlyDayString: string, nightlyNumberString: string) =
    match (UInt16.TryParse betaNumberString, Byte.TryParse nightlyDayString, UInt16.TryParse nightlyNumberString) with
    | (true, betaNumber), (true, nightlyDay), (true, nightlyNumber) ->
        let betaNightly = Build.BetaNightly ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })
        Ok betaNightly
    | _ -> Error BuildInvalidString
    
let private tryCreateBetaBuild (numberString: string) =
    match UInt16.TryParse numberString with
    | true, buildNumber ->
        let beta = Build.Beta { Number = buildNumber }
        Ok beta
    | _ -> Error BuildInvalidString
    
let private tryCreateAlphaNightlyBuild (alphaNumberString: string, nightlyDayString: string, nightlyNumberString: string) =
    match (UInt16.TryParse alphaNumberString, Byte.TryParse nightlyDayString, UInt16.TryParse nightlyNumberString) with
    | (true, alphaNumber), (true, nightlyDay), (true, nightlyNumber) ->
        let alphaNightly = Build.AlphaNightly ({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber })
        Ok alphaNightly
    | _ -> Error BuildInvalidString
    
let private tryCreateAlphaBuild (numberString: string) =
    match UInt16.TryParse numberString with
    | true, buildNumber ->
        let beta = Build.Alpha { Number = buildNumber }
        Ok beta
    | _ -> Error BuildInvalidString
    
let private tryCreateNightlyBuild (dayString: string, numberString: string) =
    match (Byte.TryParse dayString, UInt16.TryParse numberString) with
    | (true, buildDay), (true, buildNumber) ->
        let nightly = Build.Nightly { Day = buildDay; Number = buildNumber }
        Ok nightly
    | _ -> Error BuildInvalidString

let private tryCreateBuild (buildString: string) =
    result {
        if isEmptyString buildString
        then
            return None
        else
            match buildString with
            | ReleaseCandidateNightly (rcNumber, nightlyDay, nightlyNumber) ->                
                let! rcNightlyBuild = tryCreateReleaseCandidateNightlyBuild (rcNumber, nightlyDay, nightlyNumber)
                return Some rcNightlyBuild
            | ReleaseCandidate number ->
                let! rcBuild = tryCreateReleaseCandidateBuild number
                return Some rcBuild
            | BetaNightly (betaNumber, nightlyDay, nightlyNumber) ->                
                let! betaNightlyBuild = tryCreateBetaNightlyBuild (betaNumber, nightlyDay, nightlyNumber)
                return Some betaNightlyBuild
            | Beta number ->
                let! betaBuild = tryCreateBetaBuild number
                return Some betaBuild
            | AlphaNightly (alphaNumber, nightlyDay, nightlyNumber) ->
                let! alphaNightlyBuild = tryCreateAlphaNightlyBuild (alphaNumber, nightlyDay, nightlyNumber)
                return Some alphaNightlyBuild
            | Alpha number ->
                let! alphaBuild = tryCreateAlphaBuild number
                return Some alphaBuild
            | Nightly (day, number) ->
                let! nightlyBuild = tryCreateNightlyBuild (day, number)
                return Some nightlyBuild            
            | _ ->
                return! Error BuildInvalidString
    }

let private createReleaseCandidate =
    Build.ReleaseCandidate { Number = NumberStartValue }
    
let private createBeta =
    Build.Beta { Number = NumberStartValue }
    
let private createAlpha =
    Build.Alpha { Number = NumberStartValue }
    
let private createNightly dayOfMonth =
    Build.Nightly { Day = dayOfMonth; Number = NumberStartValue }
    
let private applyNightly (build, dayOfMonth) =
    match build with
        | Build.ReleaseCandidate { Number = rcNumber } ->
            Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = dayOfMonth; Number = NumberStartValue })
        | Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = nightlyDay; Number = nightlyNumber }) when nightlyDay = dayOfMonth ->
            Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = dayOfMonth; Number = nextNumber nightlyNumber })
        | Build.ReleaseCandidateNightly ({ Number = rcNumber }, _) ->
            Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = dayOfMonth; Number = NumberStartValue } )
        | Build.Beta { Number = betaNumber } ->
            Build.BetaNightly ({ Number = betaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
        | Build.BetaNightly ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber }) when nightlyDay = dayOfMonth  ->
            Build.BetaNightly ({ Number = betaNumber }, { Day = nightlyDay; Number = nextNumber nightlyNumber })
        | Build.BetaNightly ({ Number = betaNumber }, _) ->
            Build.BetaNightly ({ Number = betaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
        | Build.Alpha { Number = alphaNumber } ->
            Build.AlphaNightly ({ Number = alphaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
        | Build.AlphaNightly ({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber }) when nightlyDay = dayOfMonth ->
            Build.AlphaNightly ({ Number = alphaNumber }, { Day = nightlyDay; Number = nextNumber nightlyNumber })
        | Build.AlphaNightly ({ Number = alphaNumber }, _) ->
            Build.AlphaNightly ({ Number = alphaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
        | Build.Nightly { Day = currentDay; Number = number } when currentDay = dayOfMonth ->
            Build.Nightly { Day = dayOfMonth; Number = nextNumber number }
        | Build.Nightly _ ->
            Build.Nightly { Day = dayOfMonth; Number = NumberStartValue }

let private applyReleaseCandidate build =
    match build with
    | Build.ReleaseCandidateNightly ({ Number = rcNumber }, _)
    | Build.ReleaseCandidate { Number = rcNumber } ->
        Build.ReleaseCandidate { Number = nextNumber rcNumber }    
    | Build.BetaNightly _
    | Build.Beta _
    | Build.Alpha _
    | Build.AlphaNightly _
    | Build.Nightly _ ->
        Build.ReleaseCandidate { Number = NumberStartValue }
        
let private tryApplyBeta build =
    match build with
    | Build.ReleaseCandidate _
    | Build.ReleaseCandidateNightly _ ->
        Error BuildDowngradeProhibited
    | Build.BetaNightly ({ Number = betaNumber }, _)
    | Build.Beta { Number = betaNumber } ->
        Ok (Build.Beta { Number = nextNumber betaNumber })
    | Build.Alpha _
    | Build.AlphaNightly _
    | Build.Nightly _ ->
        Ok (Build.Beta { Number = NumberStartValue })
        
let private tryApplyAlpha build =
    match build with
    | Build.ReleaseCandidateNightly _
    | Build.ReleaseCandidate _    
    | Build.BetaNightly _
    | Build.Beta _ ->
        Error BuildDowngradeProhibited
    | Build.Alpha { Number = alphaNumber }
    | Build.AlphaNightly ({ Number = alphaNumber }, _) ->
        Ok (Build.Alpha { Number = nextNumber alphaNumber })
    | Build.Nightly _ ->
        Ok (Build.Alpha { Number = NumberStartValue })
    
let toString (build: Build) : string =
    match build with
    | Build.ReleaseCandidateNightly ({ Number = rcNumber }, { Day = nightlyDay; Number = nightlyNumber }) ->
        $"{ReleaseCandidateBuildType}{BuildTypeNumberDivider}{rcNumber}{PreReleaseNightlyDivider}{nightlyDay}{DayNumberDivider}{nightlyNumber}"
    | Build.ReleaseCandidate { Number = number } ->
        $"{ReleaseCandidateBuildType}{BuildTypeNumberDivider}{number}"
    | Build.BetaNightly ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber }) ->
        $"{BetaBuildType}{BuildTypeNumberDivider}{betaNumber}{PreReleaseNightlyDivider}{nightlyDay}{DayNumberDivider}{nightlyNumber}"
    | Build.Beta { Number = number } ->
        $"{BetaBuildType}{BuildTypeNumberDivider}{number}"
    | Build.AlphaNightly ({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber }) ->
        $"{AlphaBuildType}{BuildTypeNumberDivider}{alphaNumber}{PreReleaseNightlyDivider}{nightlyDay}{DayNumberDivider}{nightlyNumber}"
    | Build.Alpha { Number = number } ->
        $"{AlphaBuildType}{BuildTypeNumberDivider}{number}"
    | Build.Nightly { Day = day; Number = number } ->
        $"{NightlyZeroPrefix}{NightlyZeroBuildTypeDivider}{NightlyBuildType}{BuildTypeDayDivider}{day}{DayNumberDivider}{number}"
    
let tryParseFromString (build: string) =
    tryCreateBuild build

let tryReleaseCandidate (currentBuild: Build option) : Result<Build, DomainError> =
    match currentBuild with
    | None -> createReleaseCandidate |> Ok
    | Some build -> applyReleaseCandidate build |> Ok
    
let tryBeta (currentBuild: Build option) : Result<Build, DomainError> =
    match currentBuild with
    | None -> createBeta |> Ok
    | Some build -> tryApplyBeta build
    
let tryAlpha (currentBuild: Build option) : Result<Build, DomainError> =
    match currentBuild with
    | None -> createAlpha |> Ok
    | Some build -> tryApplyAlpha build

let tryNightly (currentBuild: Build option) (dayOfMonth: DayOfMonth) : Result<Build, DomainError> =        
    match currentBuild with
    | None -> createNightly dayOfMonth |> Ok
    | Some build -> applyNightly (build, dayOfMonth) |> Ok