// Domain Errors
namespace Calaf.Domain.Errors

type InitError =
    | CannotCreateProject of path: string
    
type BumpProjectError =
    | NoCalendarVersionProject
    | CannotUpdateVersionElement of name: string
    | UnversionedProject
    | AlreadyBumpedProject
    | SkippedProject


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
    | Init       of InitError
    | Bump       of BumpProjectError
    | FileSystem of FileSystemError
    | Xml        of XmlError
    | Api        of ApiError
    