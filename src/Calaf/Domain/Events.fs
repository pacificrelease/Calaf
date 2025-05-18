namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository events
type RepositoryCreated = {    
    Version: Version option
    State: RepositoryState
}

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | RepositoryCreated of RepositoryCreated
    | RepositoryBumped  of RepositoryCalendarVersionBumped

    
// Suite events
type SuiteCreated = {
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
    | SuiteCreated of SuiteCreated
    | SuiteBumped  of SuiteBumped
    
// Workspace events
type WorkspaceCreated = {
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
    | WorkspaceCreated of WorkspaceCreated
    | WorkspaceBumped  of WorkspaceBumped
    
type DomainEvent =
    | Repository of RepositoryEvent
    | Suite      of SuiteEvent
    | Workspace  of WorkspaceEvent