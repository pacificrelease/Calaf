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
        | Tag.Versioned(name, CalVer _, commitOption) ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.semVerGitTagInfo> |])>]
    let ``SemVer-named tag always creates Tag.Versioned with the corresponding SemVer version`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Versioned(name, SemVer _, commitOption) ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.malformedGitTagInfo> |])>]
    let ``Malform-named tag always creates Tag.Unversioned with the corresponding malformed name`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Unversioned name ->
            name = contract.Name
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calVerOrSemVerWithCommitGitTagInfo> |])>]
    let ``CalVer or SemVer named tag with the commit option creates Tag.Versioned with the corresponding commit option`` (contract: GitTagInfo) =
        match create contract, contract.Commit with
        | Tag.Versioned(_, _, Some commit), Some expectedCommit ->
            expectedCommit.Message = commit.Message &&
            expectedCommit.Hash = commit.Hash &&
            expectedCommit.When = commit.When
        | Tag.Versioned(_, _, None), None ->
            true
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.randomGitTagInfo> |])>]
    let ``Tag never creates unexpected DU cases`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Versioned _ | Tag.Unversioned _ -> true
        
module ChooseCalendarVersionsPropertiesTests =
    [<Property>]
    let ``Empty tags yields empty CalendarVersions sequence`` () =
        chooseCalendarVersions Seq.empty |> Seq.isEmpty
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calendarVersionsTags> |])>]
    let ``Only CalendarVersions tags yields same quantity of the CalendarVersions seq`` (calendarVersionsTags: Tag[]) =
        let calendarVersions = chooseCalendarVersions calendarVersionsTags
        (calendarVersions |> Seq.length) = (calendarVersionsTags |> Seq.length)