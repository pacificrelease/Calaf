namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository
type RepositoryCreated = {    
    Version: Version
    State: RepositoryState
}

type CalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
    State: RepositoryState
}

type RepositoryEvent =
    | RepositoryCreated of RepositoryCreated
    | RepositoryBumped  of CalendarVersionBumped 