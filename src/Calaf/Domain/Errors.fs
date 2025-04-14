// Domain Errors
namespace Calaf.Domain.Errors

open Calaf.Domain.DomainTypes

type DomainError =
    | InitWorkspaceError of desc: string
    | BumpVersionError of desc: string * currentVersion: Version * timeStamp: System.DateTime