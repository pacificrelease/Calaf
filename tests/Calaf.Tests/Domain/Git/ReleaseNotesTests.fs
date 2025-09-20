namespace Calaf.Tests.ChangelogTests

open FsCheck.Xunit
open Swensen.Unquote

open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Tests

module TryCreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FeatList>; typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Feat commits list creates features only changeset with the corresponding event``
        (commits: Commit list)
        (calendarVersion: CalendarVersion)
        (timeStamp: System.DateTimeOffset)=
        let releaseNotes = ReleaseNotes.tryCreate commits calendarVersion timeStamp       
        match releaseNotes with
        | Some (WithChanges { Features = features; Fixes = fixes; Other = other; Version = rnCalendarVersion; TimeStamp = rnTimeStamp }, events) ->
            test <@
                not features.IsEmpty &&
                fixes.IsEmpty &&
                other.IsEmpty &&
                rnCalendarVersion = calendarVersion &&
                rnTimeStamp = timeStamp &&
                events.Length = 1
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.FixList>; typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Fix commits list creates fixes only changelog with the corresponding event``
        (commits: Commit list)
        (calendarVersion: CalendarVersion)
        (timeStamp: System.DateTimeOffset)=
        let releaseNotes = ReleaseNotes.tryCreate commits calendarVersion timeStamp       
        match releaseNotes with
        | Some (WithChanges { Features = features; Fixes = fixes; Other = other; Version = rnCalendarVersion; TimeStamp = rnTimeStamp }, events) ->
            test <@
                not fixes.IsEmpty &&
                features.IsEmpty &&
                other.IsEmpty &&
                rnCalendarVersion = calendarVersion &&
                rnTimeStamp = timeStamp &&
                events.Length = 1
            @>
        | _ -> test <@ false @>
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.Commit.OtherList>; typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Other commits list creates others only changelog``
        (commits: Commit list)
        (calendarVersion: CalendarVersion)
        (timeStamp: System.DateTimeOffset)=
        let changelog = ReleaseNotes.tryCreate commits calendarVersion timeStamp      
        match changelog with
        | Some (WithChanges { Features = features; Fixes = fixes; Other = other; BreakingChanges = breakingChanges; Version = rnCalendarVersion; TimeStamp = rnTimeStamp }, events) ->
            not other.IsEmpty &&
            features.IsEmpty &&
            fixes.IsEmpty &&
            breakingChanges.IsEmpty &&
            rnCalendarVersion = calendarVersion &&
            rnTimeStamp = timeStamp &&
            events.Length = 1
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.CalendarVersion.Accidental> |])>]
    let ``Empty commits list always creates empty changeset``
        (calendarVersion: CalendarVersion)
        (timeStamp: System.DateTimeOffset) =        
        let changelog = ReleaseNotes.tryCreate [] calendarVersion timeStamp        
        match changelog with
        | Some (WithoutChanges (rnCalendarVersion, rnTimeStamp), events) -> 
            rnCalendarVersion = calendarVersion &&
            rnTimeStamp = timeStamp &&
            events.Length = 1            
        | _ -> false
        
module ToStringPropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.ReleaseNotes.FeaturesChangeset> |])>]
    let ``Test toString on features only changeset returns non-empty string``
        (releaseNotes: ReleaseNotes)=
        let changeset = ReleaseNotes.toString releaseNotes 
        test <@ System.String.IsNullOrWhiteSpace changeset |> not @>