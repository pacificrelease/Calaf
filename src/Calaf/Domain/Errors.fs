// Domain Errors
namespace Calaf.Domain.Errors

open Calaf.Domain.DomainTypes

type DomainError =
    | InitWorkspaceError         of msg: string
    | UnversionedProjectError    of project: Project
    | InvalidProjectVersionError of project: Project
    | BumpedProjectError         of project: Project