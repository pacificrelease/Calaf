namespace Calaf.Tests.CommitMessageTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain.DomainTypes
open Calaf.Domain.CommitMessage
open Calaf.Tests

module CreatePropertiesTests =
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.FeatNonBreakingChangeString> |])>]
    let ``Feature non-breaking change commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Feature feat ->
                feat.BreakingChange = false
            | _ -> false
        test <@ isOk @>
        
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.FeatBreakingChangeString> |])>]
    let ``Feature breaking change commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Feature feat ->
                feat.BreakingChange = true
            | _ -> false
        test <@ isOk @>
        
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.FixNonBreakingChangeString> |])>]
    let ``Fix non-breaking change commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Fix fix ->
                fix.BreakingChange = false
            | _ -> false
        test <@ isOk @>
        
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.FixBreakingChangeString> |])>]
    let ``Fix breaking change commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Fix fix ->
                fix.BreakingChange = true
            | _ -> false
        test <@ isOk @>
        
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.OtherString> |])>]
    let ``Other commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Other msg ->
                msg = commitText
            | _ -> false
        test <@ isOk @>
        
    [<Property( Arbitrary = [| typeof<Arbitrary.Git.CommitMessage.EmptyString> |])>]
    let ``Empty commit text always creates corresponding CommitMessage`` (commitText: CommitText) =
        let commitMessage = create commitText
        let isOk =
            match commitMessage with
            | Empty -> true                
            | _ -> false
        test <@ isOk @>
    