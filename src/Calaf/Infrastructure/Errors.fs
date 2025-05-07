namespace Calaf.Infrastructure

type GitError =
    | RepoNotInitialized
    | RepoAccessFailed of ex: exn    
    
type FileSystemError =
    | DirectoryDoesNotExist
    | DirectoryAccessDenied of ex: exn
    | FileAccessDenied      of ex: exn
    | FilesScanFailed       of ex: exn
    | XmlLoadFailed         of ex: exn
    | XmlSaveFailed         of ex: exn
    
type InfrastructureError =
    | Git        of GitError
    | FileSystem of FileSystemError
    