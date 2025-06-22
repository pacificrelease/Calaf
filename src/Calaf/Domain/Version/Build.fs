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
let internal BuildTypeDayDivider = "."
[<Literal>]
let internal DayNumberDivider = "."
let private allowedNightlyBuildRegexString =
    $@"^(?i:({NightlyBuildType}))\{BuildTypeDayDivider}([1-9]|[12][0-9]|3[0-1])\{DayNumberDivider}([1-9][0-9]{{0,4}})$"
let private matchBuildRegex (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, allowedNightlyBuildRegexString)
    
type private BuildSegments = {
    BuildType:   string
    BuildDay:    string
    BuildNumber: string
}

let private isEmptyString (build: string) =
    String.IsNullOrWhiteSpace(build)
    
let private isNightlyString (buildType: string) =
    String.Equals(buildType, NightlyBuildType, StringComparison.InvariantCultureIgnoreCase)    
    
let private tryCreateBuildSegments (buildString: string) =
    result {
        if isEmptyString buildString
        then
            return None
        else
            let m = matchBuildRegex buildString
            if m.Success
            then
                let segments =
                    { BuildType   = m.Groups[1].Value
                      BuildDay    = m.Groups[2].Value
                      BuildNumber = m.Groups[3].Value }
                return Some segments                
            else        
                return! Error BuildInvalidString
    }

let private tryParseFromBuildSegments segments =
    match segments with    
    | Some { BuildType = buildType; BuildDay = day; BuildNumber = number }
        when buildType |> isNightlyString ->
        match (Byte.TryParse day, UInt16.TryParse number) with
        | (true, buildDay), (true, buildNumber) ->
            let nightly = { Day = buildDay; Number = buildNumber } |> Build.Nightly
            nightly |> Some |> Ok
        | _ -> Error BuildInvalidString
    | None -> Ok None
    | _ -> Error BuildInvalidString
    
let toString (build: Build) : string =    
    match build with
    | Build.Nightly { Day = day; Number = number } ->
        $"{NightlyBuildType}{BuildTypeDayDivider}{day}{DayNumberDivider}{number}"
    
let tryParseFromString (build: string) =
    build |> tryCreateBuildSegments |> Result.bind tryParseFromBuildSegments 

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