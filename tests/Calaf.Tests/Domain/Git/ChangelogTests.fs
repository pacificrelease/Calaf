namespace Calaf.Tests.ChangelogTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Tests

module TryCreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FeatList> |])>]
    let ``Feat commits list creates features only changelog``
        (commits: Commit list) =
        let changelog = VersionLog.tryCreate commits
        let allFeat =
            match changelog with
            | Some ch ->
                not ch.Features.IsEmpty &&
                ch.Fixes.IsEmpty
            | _ -> false                
        test <@ allFeat @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FixList> |])>]
    let ``Fix commits list creates fixes only changelog``
        (commits: Commit list) =
        let changelog = VersionLog.tryCreate commits
        let allFix =
            match changelog with
            | Some ch ->
                not ch.Fixes.IsEmpty &&
                ch.Features.IsEmpty
            | _ -> false                
        test <@ allFix @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.OtherList> |])>]
    let ``Other commits list creates others only changelog``
        (commits: Commit list) =
        let changelog = VersionLog.tryCreate commits
        let allOther =
            match changelog with
            | Some ch ->
                ch.Features.IsEmpty &&
                ch.Fixes.IsEmpty                
            | _ -> false                
        test <@ allOther @>