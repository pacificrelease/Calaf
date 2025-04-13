namespace Calaf.Errors

type DomainError =
    | InitWorkspaceError of desc: string
