module internal Calaf.Domain.CommitMessage

open Calaf.Domain.DomainTypes

[<Literal>]
let internal FeaturePrefix = "feat"
[<Literal>]
let internal FixPrefix = "fix"
[<Literal>]
let internal LeftParenthesis = "("
[<Literal>]
let internal RightParenthesis = ")"
[<Literal>]
let internal EndOfPattern = ":"
[<Literal>]
let internal BreakingChange = "!"
[<Literal>]
let internal BreakingChangeFooter = "BREAKING CHANGE"
[<Literal>]
let private typeGroupName = "type"
[<Literal>]
let private scopeGroupName = "scope"
[<Literal>]
let private breakingChangeGroupName = "breakingChange"
[<Literal>]
let private eopGroupName = "eop"
[<Literal>]
let private descGroupName = "desc"
let private featureNonBreakingChangeCommitPattern =
    @$"^(?i)(?<{typeGroupName}>\s*{FeaturePrefix}(?:\s+(?=\())?)(?:\((?<{scopeGroupName}>[^)]*?)\))?(?<{eopGroupName}>\s*{EndOfPattern})(?<{descGroupName}>[\s\S]*)"
let private featureBreakingChangeCommitPattern =
    @$"^(?i)(?<{typeGroupName}>\s*{FeaturePrefix}(?:\s+(?=\())?)(?:\((?<{scopeGroupName}>[^)]*?)\))?(?<{breakingChangeGroupName}>\s*{BreakingChange})(?<{eopGroupName}>\s*{EndOfPattern})(?<{descGroupName}>[\s\S]*)"
let private fixNonBreakingChangeCommitPattern =
    @$"^(?i)(?<{typeGroupName}>\s*{FixPrefix}(?:\s+(?=\())?)(?:\((?<{scopeGroupName}>[^)]*?)\))?(?<{eopGroupName}>\s*{EndOfPattern})(?<{descGroupName}>[\s\S]*)"
let private fixBreakingChangeCommitPattern =        
    @$"^(?i)(?<{typeGroupName}>\s*{FixPrefix}(?:\s+(?=\())?)(?:\((?<{scopeGroupName}>[^)]*?)\))?(?<{breakingChangeGroupName}>\s*{BreakingChange})(?<{eopGroupName}>\s*{EndOfPattern})(?<{descGroupName}>[\s\S]*)"
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
    
let private createConventionalCommitMessage
    (regexMatch: System.Text.RegularExpressions.Match, isBreakingChange: bool)=
    let scope = regexMatch.Groups[scopeGroupName].Value
    Some {
        _type = regexMatch.Groups[typeGroupName].Value
        _scope = scope
        _breakingChange =
            if isBreakingChange
            then regexMatch.Groups[breakingChangeGroupName].Value
            else System.String.Empty
        _splitter = regexMatch.Groups[eopGroupName].Value            
        Scope = scope |> toValueOrNone
        BreakingChange = isBreakingChange
        Description = regexMatch.Groups[descGroupName].Value
    }
    
let private (|FeatureNonBreakingChange|_|) (input: string) =
    let m = matchFeatureNonBreakingChangeCommitPatternRegexString input
    if m.Success then createConventionalCommitMessage (m, false)         
    else None
    
let private (|FeatureBreakingChange|_|) (input: string) =
    let m = matchFeatureBreakingChangeCommitPatternRegexString input
    if m.Success then createConventionalCommitMessage (m, true)         
    else None
    
let private (|FixNonBreakingChange|_|) (input: string) =
    let m = matchFixNonBreakingChangeCommitPatternRegexString input
    if m.Success then createConventionalCommitMessage (m, false)         
    else None
    
let private (|FixBreakingChange|_|) (input: string) =
    let m = matchFixBreakingChangeCommitPatternRegexString input
    if m.Success then createConventionalCommitMessage (m, true)         
    else None
    
let private (|NoMessage|_|) (input: string) =
    if System.String.IsNullOrWhiteSpace input then Some ()
    else None
    
let toString (commitMessage: CommitMessage) : string =
    let toScopeString scope =
        if System.String.IsNullOrEmpty scope
        then scope
        else $"{LeftParenthesis}{scope}{RightParenthesis}"
        
    match commitMessage with
    | Feature cm ->        
        $"{cm._type}{toScopeString cm._scope}{cm._breakingChange}{cm._splitter}{cm.Description}"
    | Fix cm ->
        $"{cm._type}{toScopeString cm._scope}{cm._breakingChange}{cm._splitter}{cm.Description}"
    | Other msg -> msg
    | Empty -> System.String.Empty
    
let create message =
    match message with
    | FeatureNonBreakingChange ccm  -> ccm |> CommitMessage.Feature
    | FeatureBreakingChange ccm -> ccm |> CommitMessage.Feature
    | FixNonBreakingChange ccm -> ccm |> CommitMessage.Fix
    | FixBreakingChange ccm -> ccm |> CommitMessage.Fix 
    | NoMessage -> CommitMessage.Empty
    | _ -> CommitMessage.Other message