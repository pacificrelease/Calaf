namespace Calaf.Contracts.InfrastructureErrors

type GitError =
    | NoRepo           of directory: string
    | RepoAccessDenied of ex: exn    
    
type FileSystemError =
    | NoPath       of msg: string
    | AccessDenied of ex: exn
    | ReadFailure  of ex: exn
    
type XmlError =
    | LoadFailure of ex: exn
    | SaveFailure of ex: exn
    
type InfrastructureError =
    | Git        of GitError
    | FileSystem of FileSystemError
    | Xml        of XmlError
    