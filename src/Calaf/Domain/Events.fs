namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion
    Signature: Signature
}

type RepositoryCalendarVersionSkipped = {
    Version: CalendarVersion option
}

type RepositoryEvent =
    | ReadyRepositoryBumped  of RepositoryCalendarVersionBumped
    | DirtyRepositorySkipped of RepositoryCalendarVersionSkipped