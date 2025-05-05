// Domain Errors
namespace Calaf.Domain.Errors

// TODO: Dedicate 3 main groups of errors:
// 1. Adapters/validation errors |> create values on domain boundaries
// 2. Domain errors |> domain logic errors
// 3. Infrastructure errors |> IO errors

type ValidationError =
    | OutOfRangeYear
    | OutOfRangeMonth
    | WrongInt32Year
    | WrongInt32Month
    | WrongStringYear
    | WrongStringMonth    

type InitError =
    | CannotCreateProject of path: string
    
type BumpProjectError =
    | NoCalendarVersionProject
    | CannotUpdateVersionElement of name: string
    | UnversionedProject
    | AlreadyBumpedProject
    | SkippedProject

type GitError =
    | RepositoryAccessError of ex: exn
    | NoGitRepository       of directory: string
    
type FileSystemError =
    | NotExistOrBadPath of msg: string
    | AccessPathError   of ex: exn
    | ReadProjectsError of ex: exn
    
type XmlError =
    | CannotLoadXml  of ex: exn
    | CannotSaveXml of ex: exn
    
type ApiError =
    | GivenNotBumpedProject of name: string
    | GivenUnversionedProject of name: string
    | GivenSkippedProject of name: string
    | NoPropertyGroupWorkspaceVersion
    | NoPropertyGroupNextVersion
    
type CalafError =
    | Validation of ValidationError
    | Init       of InitError
    | Bump       of BumpProjectError
    | Git        of GitError
    | FileSystem of FileSystemError
    | Xml        of XmlError
    | Api        of ApiError
    