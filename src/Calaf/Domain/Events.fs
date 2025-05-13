namespace Calaf.Domain.DomainEvents

open Calaf.Domain.DomainTypes

type RepositoryCalendarVersionBumped = {
    Version: CalendarVersion    
}

type RepositoryEvent =
    | CalendarVersionBumped of RepositoryCalendarVersionBumped