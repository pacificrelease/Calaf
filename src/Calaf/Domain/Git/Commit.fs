module internal Calaf.Domain.Commit

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values

[<Literal>]
let internal FeaturePrefix = "feat"
[<Literal>]
let internal EndOfPattern = ":"
[<Literal>]
let internal BreakingChange = "!"
let private featureNonBreakingChangeCommitPattern =        
    @$"^{FeaturePrefix}(?:\([^)]*\))?{EndOfPattern}"
let private featureBreakingChangeCommitPattern =        
    @$"^{FeaturePrefix}(?:\([^)]*\))?{BreakingChange}{EndOfPattern}"    
let private featureNonBreakingChangeCommitPatternRegexString =
    $@"^{featureNonBreakingChangeCommitPattern}"
let private featureBreakingChangeCommitPatternRegexString =
    $@"^{featureBreakingChangeCommitPattern}"
    
let private matchFeatureNonBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, featureNonBreakingChangeCommitPatternRegexString)
let private matchFeatureBreakingChangeCommitPatternRegexString (input: string) =
    System.Text.RegularExpressions.Regex.Match(input, featureBreakingChangeCommitPatternRegexString)
    
let private (|FeatureNonBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else None
    
let private (|FeatureBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then
        let numberSegment = m.Groups[2].Value
        Some numberSegment
    else None
    
let private createAdoptedCommitMessage message =
    match message with
    | FeatureNonBreakingChange m ->
        AdoptedCommitMessage.Feature (false, m)
    | FeatureBreakingChange m ->
        AdoptedCommitMessage.Feature (true, m)    
    | _ ->
        AdoptedCommitMessage.Other message

let create (commitInfo: GitCommitInfo) =
    { Message = commitInfo.Message
      Hash = commitInfo.Hash
      When = commitInfo.When }