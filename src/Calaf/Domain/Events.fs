namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes.Values

// Repository events
type RepositoryCaptured = {    
    Version: Version option
    State: RepositoryState
}

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | RepositoryCaptured of RepositoryCaptured
    | RepositoryBumped   of RepositoryCalendarVersionBumped

    
// Suite events
type SuiteCaptured = {
    CalendarVersion: CalendarVersion
    CalendarVersionProjectsCount: uint16
    TotalProjectsCount: uint16
}

type SuiteReleased = {
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    ProjectsBumpedCount: uint16
    TotalProjectsCount: uint16
}

type SuiteEvent =
    | SuiteCaptured of SuiteCaptured
    | SuiteReleased of SuiteReleased
    
// Workspace events
type WorkspaceCaptured = {
    Directory: string
    Version: CalendarVersion
    RepositoryExist: bool
    RepositoryVersion: CalendarVersion option
    SuiteVersion: CalendarVersion
}

type WorkspaceReleased = {
    Directory: string
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    RepositoryExist: bool
}

type WorkspaceEvent =
    | WorkspaceCaptured of WorkspaceCaptured
    | WorkspaceReleased of WorkspaceReleased
    
type DomainEvent =
    | Repository of RepositoryEvent
    | Suite      of SuiteEvent
    | Workspace  of WorkspaceEvent