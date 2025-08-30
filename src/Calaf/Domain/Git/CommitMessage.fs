module internal Calaf.Domain.CommitMessage

open Calaf.Domain.DomainTypes

[<Literal>]
let internal FeaturePrefix = "feat"
[<Literal>]
let internal FixPrefix = "fix"
[<Literal>]
let internal EndOfPattern = ":"
[<Literal>]
let internal BreakingChange = "!"
[<Literal>]
let internal BreakingChangeFooter = "BREAKING CHANGE"
let private featureNonBreakingChangeCommitPattern =        
    @$"^(?i){FeaturePrefix}(?:\(([^)]*)\))?{EndOfPattern}\s*(\S.*)"
let private featureBreakingChangeCommitPattern =        
    @$"^(?i){FeaturePrefix}(?:\(([^)]*)\))?{BreakingChange}{EndOfPattern}\s*(\S.*)"
let private fixNonBreakingChangeCommitPattern =
    @$"^(?i){FixPrefix}(?:\(([^)]*)\))?{EndOfPattern}\s*(\S.*)"
let private fixBreakingChangeCommitPattern =        
    @$"^(?i){FixPrefix}(?:\(([^)]*)\))?{BreakingChange}{EndOfPattern}\s*(\S.*)"
let private featureNonBreakingChangeCommitPatternRegexString =
    $@"^{featureNonBreakingChangeCommitPattern}"
let private featureBreakingChangeCommitPatternRegexString =
    $@"^{featureBreakingChangeCommitPattern}"
let private fixNonBreakingChangeCommitPatternRegexString =
    $@"^{fixNonBreakingChangeCommitPattern}"
let private fixBreakingChangeCommitPatternRegexString =
    $@"^{fixBreakingChangeCommitPattern}"
    
let private matchFeatureNonBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, featureNonBreakingChangeCommitPatternRegexString)
let private matchFeatureBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, featureBreakingChangeCommitPatternRegexString)
let private matchFixNonBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, fixNonBreakingChangeCommitPatternRegexString)
let private matchFixBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, fixBreakingChangeCommitPatternRegexString)    

let private toValueOrNone (s: string) =
    if not (System.String.IsNullOrWhiteSpace s)
    then Some s
    else None
    
let private createConventionalCommitDetails
    (scope: string)
    (description: string)
    (breakingChange: bool) =
    let scopeOption = scope |> toValueOrNone
    Some (scopeOption, description, breakingChange)

let private (|FeatureNonBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        createConventionalCommitDetails
            m.Groups[1].Value
            m.Groups[2].Value
            false
    else None
    
let private (|FeatureBreakingChange|_|) (input: string) =
    let m = matchFeatureBreakingChangeCommitPatternRegexString input
    if m.Success then
        createConventionalCommitDetails
            m.Groups[1].Value
            m.Groups[2].Value
            true
    else None
    
let private (|FixNonBreakingChange|_|) (input: string) =
    let m = matchFixNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        createConventionalCommitDetails
            m.Groups[1].Value
            m.Groups[2].Value
            false
    else None
    
let private (|FixBreakingChange|_|) (input: string) =
    let m = matchFixNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        createConventionalCommitDetails
            m.Groups[1].Value
            m.Groups[2].Value
            true
    else None
    
let private (|NoMessage|_|) (input: string) =
    if System.String.IsNullOrWhiteSpace input then
        Some ()
    else None
    
let create message =
    let createConventionalCommitMessage scope desc breakingChange =
        { Scope = scope; Description = desc; BreakingChange = breakingChange }        
    match message with
    | FeatureNonBreakingChange (scope, desc, breakingChange)  ->
        createConventionalCommitMessage scope desc breakingChange |> CommitMessage.Feature
    | FeatureBreakingChange (scope, desc, breakingChange) ->
        createConventionalCommitMessage scope desc breakingChange |> CommitMessage.Feature
    | FixNonBreakingChange (scope, desc, breakingChange) ->
        createConventionalCommitMessage scope desc breakingChange |> CommitMessage.Fix
    | FixBreakingChange (scope, desc, breakingChange) ->
        createConventionalCommitMessage scope desc breakingChange |> CommitMessage.Fix 
    | NoMessage ->
        CommitMessage.Empty
    | _ ->
        CommitMessage.Other message