namespace Calaf.Application

open Calaf.Domain
open Calaf.Infrastructure

type CalafError =
    | Domain         of DomainError
    | Infrastructure of InfrastructureError