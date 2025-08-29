module internal Calaf.Domain.Commit

open Calaf.Contracts
open Calaf.Domain.DomainTypes

[<Literal>]
let internal FeaturePrefix = "feat"
[<Literal>]
let internal FixPrefix = "fix"
[<Literal>]
let internal EndOfPattern = ":"
[<Literal>]
let internal BreakingChange = "!"
let private featureNonBreakingChangeCommitPattern =        
    @$"^{FeaturePrefix}(?:\(([^)]*)\))?{EndOfPattern}\s*(.*)"
let private featureBreakingChangeCommitPattern =        
    @$"^{FeaturePrefix}(?:\(([^)]*)\))?{BreakingChange}{EndOfPattern}\s*(.*)"
let private fixNonBreakingChangeCommitPattern =
    @$"^{FixPrefix}(?:\(([^)]*)\))?{EndOfPattern}\s*(.*)"
let private fixBreakingChangeCommitPattern =        
    @$"^{FixPrefix}(?:\(([^)]*)\))?{BreakingChange}{EndOfPattern}\s*(.*)"
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
    then s.Trim() |> Some
    else None
    
let private (|FeatureNonBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let scope       = m.Groups[1].Value |> toValueOrNone
        let description = m.Groups[2].Value |> toValueOrNone
        Some (scope, description)
    else None
    
let private (|FeatureBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let scope       = m.Groups[1].Value |> toValueOrNone
        let description = m.Groups[2].Value |> toValueOrNone
        Some (scope, description)
    else None
    
let private (|FixNonBreakingChange|_|) (input: string) =
    let m = matchFixNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let scope       = m.Groups[1].Value |> toValueOrNone
        let description = m.Groups[2].Value |> toValueOrNone
        Some (scope, description)
    else None
    
let private (|FixBreakingChange|_|) (input: string) =
    let m = matchFixNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let scope       = m.Groups[1].Value |> toValueOrNone
        let description = m.Groups[2].Value |> toValueOrNone
        Some (scope, description)
    else None
    
let createCommitMessage message =
    let nonBreakingChange = false
    let breakingChange = true
    match message with
    | FeatureNonBreakingChange (s, m) ->        
        CommitMessage.Feature (nonBreakingChange, s, m)
    | FeatureBreakingChange (s, m) ->
        CommitMessage.Feature (breakingChange, s, m)
    | FixNonBreakingChange (s, m) ->
        CommitMessage.Fix (nonBreakingChange, s, m)
    | FixBreakingChange (s, m) ->
        CommitMessage.Fix (breakingChange, s, m)
    | _ ->
        CommitMessage.Other (toValueOrNone message)

let create (commitInfo: GitCommitInfo) =
    let message = createCommitMessage commitInfo.Text
    { Message = message
      Text = commitInfo.Text
      Hash = commitInfo.Hash
      When = commitInfo.When }