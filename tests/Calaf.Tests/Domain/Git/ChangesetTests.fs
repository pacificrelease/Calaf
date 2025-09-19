namespace Calaf.Tests.ChangelogTests

open Xunit
open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Tests

module TryCreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FeatList> |])>]
    let ``Feat commits list creates features only changeset with the corresponding event``
        (commits: Commit list)
        (timeStamp: System.DateTimeOffset)=
        let changeset = Changeset.tryCreate commits timeStamp       
        match changeset with
        | Some (chs, events) ->
            test <@
                not chs.Features.IsEmpty &&
                chs.Fixes.IsEmpty &&
                chs.Other.IsEmpty &&
                chs.TimeStamp = timeStamp &&
                events.Length = 1
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FixList> |])>]
    let ``Fix commits list creates fixes only changelog with the corresponding event``
        (commits: Commit list)
        (timeStamp: System.DateTimeOffset)=
        let changelog = Changeset.tryCreate commits timeStamp       
        match changelog with
        | Some (chs, events) ->
            test <@
                not chs.Fixes.IsEmpty &&
                chs.Features.IsEmpty &&
                chs.Other.IsEmpty &&
                chs.TimeStamp = timeStamp &&
                events.Length = 1
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.OtherList> |])>]
    let ``Other commits list creates others only changelog``
        (commits: Commit list)
        (timeStamp: System.DateTimeOffset)=
        let changelog = Changeset.tryCreate commits timeStamp      
        match changelog with
        | Some (chs, events) ->
            not chs.Other.IsEmpty &&
            chs.Features.IsEmpty &&
            chs.Fixes.IsEmpty &&
            chs.BreakingChanges.IsEmpty &&
            chs.TimeStamp = timeStamp &&
            events.Length = 1
        | _ -> false
        
    [<Property>]
    let ``Empty commits list always creates empty changeset``
        (timeStamp: System.DateTimeOffset) =
        let changelog = Changeset.tryCreate [] timeStamp        
        match changelog with
        | None ->
            true
        | _ -> false
        
module ToStringPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Changeset.FeaturesChangeset>; typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Test``
        (changeset: Changeset) (calendarVersion: CalendarVersion)=
        let changeset = Changeset.toString changeset calendarVersion 
        test <@ System.String.IsNullOrWhiteSpace changeset |> not @>