namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository events
type RepositoryStateCaptured = {    
    Version: Version option
    State: RepositoryState
}

type RepositoryChangesetCaptured = {
    Changeset: Changeset
}

type RepositoryReleaseProvided = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | StateCaptured               of RepositoryStateCaptured
    | RepositoryChangesetCaptured of RepositoryChangesetCaptured
    | ReleaseProvided             of RepositoryReleaseProvided

    
// Collection events
type CollectionStateCaptured = {
    CalendarVersion: CalendarVersion
    CalendarVersionProjectsCount: uint16
    TotalProjectsCount: uint16
}

type CollectionReleaseCreated = {
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    ProjectsBumpedCount: uint16
    TotalProjectsCount: uint16
}

type CollectionEvent =
    | StateCaptured  of CollectionStateCaptured
    | ReleaseCreated of CollectionReleaseCreated
    
// Workspace events
type WorkspaceStateCaptured = {
    Directory: string
    Version: CalendarVersion
    RepositoryExist: bool
    RepositoryVersion: CalendarVersion option
    CollectionVersion: CalendarVersion
}

type WorkspaceReleaseCreated = {
    Directory: string
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    RepositoryExist: bool
}

type WorkspaceEvent =
    | StateCaptured  of WorkspaceStateCaptured
    | ReleaseCreated of WorkspaceReleaseCreated
    
type DomainEvent =
    | Repository of RepositoryEvent
    | Collection of CollectionEvent
    | Workspace  of WorkspaceEvent