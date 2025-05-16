namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository
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

    
// Suite
type SuiteCreated = {
    CalendarVersion: CalendarVersion option
    CalendarVersionProjectsCount: uint16
    TotalProjectsCount: uint16
}

type SuiteEvent =
    | SuiteCreated of SuiteCreated
    
type DomainEvent =
    | Repository of RepositoryEvent
    | Suite      of SuiteEvent