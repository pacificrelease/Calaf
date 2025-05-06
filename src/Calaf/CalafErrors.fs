namespace Calaf.CalafErrors

open Calaf.Contracts.InfrastructureErrors
open Calaf.Domain.DomainErrors

type CalafError =
    | Domain         of DomainError
    | Infrastructure of InfrastructureError