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

type SuiteBumped = {
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    ProjectsBumpedCount: uint16
    TotalProjectsCount: uint16
}

type SuiteEvent =
    | SuiteCaptured of SuiteCaptured
    | SuiteBumped   of SuiteBumped
    
// Workspace events
type WorkspaceCaptured = {
    Directory: string
    Version: CalendarVersion
    RepositoryExist: bool
    RepositoryVersion: CalendarVersion option
    SuiteVersion: CalendarVersion
}

type WorkspaceBumped = {
    Directory: string
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    RepositoryExist: bool
}

type WorkspaceEvent =
    | WorkspaceCaptured of WorkspaceCaptured
    | WorkspaceBumped   of WorkspaceBumped
    
type DomainEvent =
    | Repository of RepositoryEvent
    | Suite      of SuiteEvent
    | Workspace  of WorkspaceEvent