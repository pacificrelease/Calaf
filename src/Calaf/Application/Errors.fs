namespace Calaf.Application

open Calaf.Domain

type ArgumentError =
    | CommandNotRecognized   of command: string
    | BuildFlagNotRecognized of flag: string    
    

type ValidationError =
    | EmptyDotNetXmlFilePattern
    | ZeroTagQuantity

    
type GitError =
    | RepoNotInitialized
    | RepoAccessFailed of ex: exn    
    
type FileSystemError =
    | DirectoryDoesNotExist
    | DirectoryAccessDenied of ex: exn
    | FileAccessDenied      of absolutePath: string * ex: exn
    | FilesScanFailed       of ex: exn
    | XmlLoadFailed         of absolutePath: string * ex: exn
    | XmlSaveFailed         of absolutePath: string * ex: exn
    
type InfrastructureError =
    | Git        of GitError
    | FileSystem of FileSystemError

    
type CalafError =
    | Argument
    | Validation     of ValidationError
    | Domain         of DomainError    
    | Infrastructure of InfrastructureError