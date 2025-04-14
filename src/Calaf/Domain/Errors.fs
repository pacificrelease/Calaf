// Domain Errors
namespace Calaf.Domain.Errors

type BumpProjectError =
    | NoCalendarVersionProject
    | UnversionedProject
    | AlreadyBumpedProject
    
type FileSystemError =
    | NotExistOrBadPath of msg: string
    | AccessPathError   of ex: exn
    
type XmlError =
    | ReadXmlError of ex: exn
    
type CalafError =
    | Bump       of BumpProjectError
    | FileSystem of FileSystemError
    | Xml        of XmlError
    