namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

// Repository
type RepositoryCreated = {    
    Version: Version
}

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
}

type RepositoryEvent =
    | RepositoryCreated of RepositoryCreated
    | RepositoryBumped  of RepositoryCalendarVersionBumped
    