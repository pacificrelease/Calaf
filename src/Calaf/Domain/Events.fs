namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository events
type RepositoryStateCaptured = {    
    Version: Version option
    State: RepositoryState
}

type RepositoryReleaseProvided = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | StateCaptured   of RepositoryStateCaptured
    | ReleaseProvided of RepositoryReleaseProvided

    
// Suite events
type SuiteStateCaptured = {
    CalendarVersion: CalendarVersion
    CalendarVersionProjectsCount: uint16
    TotalProjectsCount: uint16
}

type SuiteReleaseCreated = {
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    ProjectsBumpedCount: uint16
    TotalProjectsCount: uint16
}

type SuiteEvent =
    | StateCaptured  of SuiteStateCaptured
    | ReleaseCreated of SuiteReleaseCreated
    
// Workspace events
type WorkspaceStateCaptured = {
    Directory: string
    Version: CalendarVersion
    RepositoryExist: bool
    RepositoryVersion: CalendarVersion option
    SuiteVersion: CalendarVersion
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
    | Suite      of SuiteEvent
    | Workspace  of WorkspaceEvent