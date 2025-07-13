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
let private allowedNightlyBuildRegexString =
    $@"^(?i:({NightlyBuildType}))\{BuildTypeDayDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
let private allowedBetaBuildRegexString =
    $@"^(?i:({BetaBuildType}))\{BuildTypeNumberDivider}([1-9][0-9]{{0,4}})$"
let private matchNightlyBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedNightlyBuildRegexString)
    
let private matchBetaBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedBetaBuildRegexString)

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
        
let private tryCreateNightlyBuild (dayString: string, numberString: string) =
    match (Byte.TryParse dayString, UInt16.TryParse numberString) with
    | (true, buildDay), (true, buildNumber) ->
        let nightly = Build.Nightly { Day = buildDay; Number = buildNumber }
        Ok nightly
    | _ -> Error BuildInvalidString
    
let private tryCreateBetaBuild (numberString: string) =
    match UInt16.TryParse numberString with
    | true, buildNumber ->
        let beta = Build.Beta { Number = buildNumber }
        Ok beta
    | _ -> Error BuildInvalidString

let private tryCreateBuild (buildString: string) =
    result {
        if isEmptyString buildString
        then
            return None
        else
            // let nightlyBuild = matchNightlyBuildRegex buildString
            // let betaBuild    = matchBetaBuildRegex buildString
            //
            // match (nightlyBuild.Success, betaBuild.Success) with
            // | true, false ->
            //     let daySegment = nightlyBuild.Groups[2].Value
            //     let numberSegment = nightlyBuild.Groups[3].Value
            //     let! nightlyBuild = tryCreateNightlyBuild (daySegment, numberSegment)
            //     return Some nightlyBuild
            // | false, true ->
            //     let numberSegment = betaBuild.Groups[2].Value
            //     let! betaBuild = tryCreateBetaBuild numberSegment                
            //     return Some betaBuild
            // | _ ->
            //     return! Error BuildInvalidString
            match buildString with
            | Nightly (day, number) ->
                let! nightlyBuild = tryCreateNightlyBuild (day, number)
                return Some nightlyBuild
            | Beta number ->
                let! betaBuild = tryCreateBetaBuild number
                return Some betaBuild
            | _ ->
                return! Error BuildInvalidString
    }
    
let toString (build: Build) : string =    
    match build with
    | Build.Nightly { Day = day; Number = number } ->
        $"{NightlyBuildType}{BuildTypeDayDivider}{day}{DayNumberDivider}{number}"
    | Build.Beta { Number = number } ->
        $"{BetaBuildType}{BuildTypeNumberDivider}{number}"
    
let tryParseFromString (build: string) =
    tryCreateBuild build

let nightly (currentBuild: Build option) (dayOfMonth: DayOfMonth) : Build =
    let nextNumber currentNumber =
        let overflowPossible = currentNumber = BuildNumber.MaxValue
        if overflowPossible
        then NumberStartValue
        else currentNumber + NumberIncrementStep
    
    match currentBuild with
    | Some (Build.Nightly { Day = currentDay; Number = number }) when currentDay = dayOfMonth ->
        Build.Nightly { Day = dayOfMonth; Number = nextNumber number }
    | _ ->
        Build.Nightly { Day = dayOfMonth; Number = NumberStartValue }