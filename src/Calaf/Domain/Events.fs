namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository events
type RepositoryStateCaptured = {    
    Version: Version option
    State: RepositoryState
}

type RepositoryReleaseNotesCaptured = {
    Notes: ReleaseNotes
}

type RepositoryReleaseProvided = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | StateCaptured        of RepositoryStateCaptured
    | ReleaseNotesCaptured of RepositoryReleaseNotesCaptured
    | ReleaseProvided      of RepositoryReleaseProvided

    
// Solution events
type SolutionStateCaptured = {
    CalendarVersion: CalendarVersion
    CalendarVersionProjectsCount: uint16
    TotalProjectsCount: uint16
}

type SolutionReleaseCreated = {
    PreviousCalendarVersion: CalendarVersion
    NewCalendarVersion: CalendarVersion
    ProjectsBumpedCount: uint16
    TotalProjectsCount: uint16
}

type SolutionEvent =
    | StateCaptured  of SolutionStateCaptured
    | ReleaseCreated of SolutionReleaseCreated
    
// Workspace events
type WorkspaceStateCaptured = {
    Directory: string
    Version: CalendarVersion
    RepositoryExist: bool
    RepositoryVersion: CalendarVersion option
    SolutionVersion: CalendarVersion
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
    | Solution   of SolutionEvent
    | Workspace  of WorkspaceEvent