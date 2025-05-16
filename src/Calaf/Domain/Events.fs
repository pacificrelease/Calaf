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
    
type DomainEvent =
    | Repository of RepositoryEvent