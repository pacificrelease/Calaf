namespace Calaf.Application

open Calaf.Domain

type InputError =
    | ArgumentsFatal of message: string
    | CommandNotRecognized  of command: string
    | ChangelogFlagRequired
    | MakeCommandMissing
    | MakeCommandNotRecognized of flag: string
    

type ValidationError =
    | EmptyDotNetXmlFilePattern
    | BadWorkspacePath of path: string
    | BadProjectPath of path: string
    | RestrictedProjectPath of path: string    
    | ZeroTagQuantity
    | EmptyChangelogFileName
    
type GitError =    
    | GitProcessErrorExit of desc: string
    | GitProcessTimeout
    | GitProcessRunFailed of ex: exn 
    | RepoNotInitialized
    | RepoAccessFailed of ex: exn
    | RepoStageNoChanges
    
type FileSystemError =
    | DirectoryDoesNotExist
    | DirectoryAccessDenied of ex: exn
    | FileAccessDenied      of absolutePath: string * ex: exn
    | FilesScanFailed       of ex: exn
    | FileGetFailed         of ex: exn
    | MarkdownLoadFailed    of absolutePath: string * ex: exn
    | MarkdownSaveFailed    of absolutePath: string * ex: exn
    | XmlLoadFailed         of absolutePath: string * ex: exn
    | XmlSaveFailed         of absolutePath: string * ex: exn
    
type InfrastructureError =
    | Input      of InputError
    | Git        of GitError
    | FileSystem of FileSystemError

    
type CalafError =
    | Validation     of ValidationError
    | Domain         of DomainError    
    | Infrastructure of InfrastructureError