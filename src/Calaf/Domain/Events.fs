namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
}

type RepositoryEvent =
    | ReadyRepositoryBumped of RepositoryCalendarVersionBumped
    