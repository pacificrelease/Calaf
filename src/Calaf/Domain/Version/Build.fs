module internal Calaf.Domain.Build

open System
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal NumberIncrementStep = 1us
[<Literal>]
let internal NumberStartValue = 1us
[<Literal>]
let private NightlyBuildType = "nightly"
[<Literal>]
let private BetaBuildType = "beta"
[<Literal>]
let internal BuildTypeDayDivider = "."
[<Literal>]
let internal DayNumberDivider = "."
[<Literal>]
let internal BuildTypeNumberDivider = "."
[<Literal>]
let internal BetaNightlyDivider = "."
let private nightlyBuildPattern =        
    $@"(?i:({NightlyBuildType}))\{BuildTypeDayDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
let private betaBuildPattern =        
    $@"(?i:({BetaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})$"
let private betaNightlyBuildPattern =        
    $@"(?i:({BetaBuildType})){BuildTypeNumberDivider}([1-9][0-9]{{0,4}})\{BetaNightlyDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
    
let private allowedNightlyBuildRegexString =
    $@"^{nightlyBuildPattern}"
let private allowedBetaBuildRegexString =
    $@"^{betaBuildPattern}"
let private allowedBetaNightlyBuildRegexString =
    $@"^{betaNightlyBuildPattern}"
    
let private matchNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedNightlyBuildRegexString)
    
let private matchBetaBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedBetaBuildRegexString)
    
let private matchBetaNightlyRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedBetaNightlyBuildRegexString)

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)
    
let private (|Nightly|_|) (input: string) =
    let m = matchNightlyBuildRegex input
    if m.Success then
        let daySegment    = m.Groups[2].Value
        let numberSegment = m.Groups[3].Value
        Some (daySegment, numberSegment)
    else
        None
        
let private (|Beta|_|) (input: string) =
    let m = matchBetaBuildRegex input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else
        None
        
let private (|BetaNightly|_|) (input: string) =
    let m = matchBetaNightlyRegex input
    if m.Success then
        let betaNumberSegment    = m.Groups[2].Value
        let nightlyDaySegment    = m.Groups[3].Value
        let nightlyNumberSegment = m.Groups[4].Value
        Some (betaNumberSegment, nightlyDaySegment, nightlyNumberSegment)
    else
        None
    
let private tryCreateNightlyBetaBuild (betaNumberString: string, nightlyDayString: string, nightlyNumberString: string) =
    match (UInt16.TryParse betaNumberString, Byte.TryParse nightlyDayString, UInt16.TryParse nightlyNumberString) with
    | (true, betaNumber), (true, nightlyDay), (true, nightlyNumber) ->
        let nightlyBeta = Build.NightlyBeta ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })
        Ok nightlyBeta
    | _ -> Error BuildInvalidString
    
let private tryCreateBetaBuild (numberString: string) =
    match UInt16.TryParse numberString with
    | true, buildNumber ->
        let beta = Build.Beta { Number = buildNumber }
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
            | BetaNightly (betaNumber, nightlyDay, nightlyNumber) ->                
                let! betaNightlyBuild = tryCreateNightlyBetaBuild (betaNumber, nightlyDay, nightlyNumber)
                return Some betaNightlyBuild
            | Beta number ->
                let! betaBuild = tryCreateBetaBuild number
                return Some betaBuild
            | Nightly (day, number) ->
                let! nightlyBuild = tryCreateNightlyBuild (day, number)
                return Some nightlyBuild            
            | _ ->
                return! Error BuildInvalidString
    }
    
let toString (build: Build) : string =
    match build with
    | Build.NightlyBeta ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber }) ->
        $"{BetaBuildType}{BuildTypeNumberDivider}{betaNumber}{BetaNightlyDivider}{nightlyDay}{DayNumberDivider}{nightlyNumber}"
    | Build.Beta { Number = number } ->
        $"{BetaBuildType}{BuildTypeNumberDivider}{number}"
    | Build.Nightly { Day = day; Number = number } ->
        $"{NightlyBuildType}{BuildTypeDayDivider}{day}{DayNumberDivider}{number}"
    
let tryParseFromString (build: string) =
    tryCreateBuild build

let nightly (currentBuild: Build option) (dayOfMonth: DayOfMonth) : Build =
    let nextNumber currentNumber =
        let overflowPossible = currentNumber = BuildNumber.MaxValue
        if overflowPossible
        then NumberStartValue
        else currentNumber + NumberIncrementStep
    
    match currentBuild with    
    | Some (Build.Beta { Number = betaNumber }) ->
        Build.NightlyBeta ({ Number = betaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
    | Some (Build.NightlyBeta ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber }))
        when nightlyDay = dayOfMonth  ->
        Build.NightlyBeta ({ Number = betaNumber }, { Day = nightlyDay; Number = nextNumber nightlyNumber })
    | Some (Build.NightlyBeta ({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })) ->
        Build.NightlyBeta ({ Number = betaNumber }, { Day = dayOfMonth; Number = NumberStartValue })
    | Some (Build.Nightly { Day = currentDay; Number = number })
        when currentDay = dayOfMonth ->
        Build.Nightly { Day = dayOfMonth; Number = nextNumber number }
    | _ ->
        Build.Nightly { Day = dayOfMonth; Number = NumberStartValue }