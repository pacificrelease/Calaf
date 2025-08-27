namespace Calaf.Tests.TagTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.Tag
open Calaf.Tests

module CreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calVerGitTagInfo> |])>]
    let ``CalVer-named tag always creates Tag.Versioned with the corresponding CalVer version`` (contract: GitTagInfo)  =
        match create contract with
        | Tag.Versioned { Name = name; Version = CalVer _; Commit = commitOption } ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.semVerGitTagInfo> |])>]
    let ``SemVer-named tag always creates Tag.Versioned with the corresponding SemVer version`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Versioned { Name = name; Version = SemVer _; Commit = commitOption } ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.malformedGitTagInfo> |])>]
    let ``Malform-named tag always creates Tag.Unversioned with the corresponding malformed name`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Unversioned name ->
            name = contract.Name
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calendarVersionOrSemanticVersionGitTagInfo> |])>]
    let ``CalVer or SemVer named tag with the commit option creates Tag.Versioned with the corresponding commit option`` (gitTagInfo: GitTagInfo) =
        match create gitTagInfo, gitTagInfo.Commit with
        | Tag.Versioned { Commit = Some commit }, Some expectedCommit ->
            expectedCommit.Text = commit.Text &&
            expectedCommit.Hash = commit.Hash &&
            expectedCommit.When = commit.When
        | Tag.Versioned { Commit = None }, None ->
            true
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.randomGitTagInfo> |])>]
    let ``Tag never creates unexpected DU cases`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Versioned _ | Tag.Unversioned _ -> true
        
module ChooseCalendarVersionsPropertiesTests =
    [<Property>]
    let ``Empty tags yields empty CalendarVersions seq`` () =
        chooseCalendarVersions Seq.empty |> Seq.isEmpty
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calendarVersionsTagsArray> |])>]
    let ``CalendarVersions tags yields same quantity of the CalendarVersions seq`` (calendarVersionsTags: Tag[]) =
        let calendarVersions = chooseCalendarVersions calendarVersionsTags
        (calendarVersions |> Seq.length) = (calendarVersionsTags |> Seq.length)
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.unversionedTagsArray> |])>]
    let ``Unversioned tags yields empty CalendarVersions seq`` (unversionedTagsArray: Tag[]) =
        chooseCalendarVersions unversionedTagsArray |> Seq.isEmpty                
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.semanticVersionsAndUnversionedTagsArray> |])>]
    let ``SemVer or Unsupported tags yields empty CalendarVersions seq`` (semanticVersionsAndUnversionedTagsArray: Tag[] ) =        
        chooseCalendarVersions semanticVersionsAndUnversionedTagsArray |> Seq.isEmpty