namespace Calaf.Application

open Calaf.Contracts


type IFileSystem =
    abstract tryReadDirectory:
        directory: string -> pattern: string -> Result<DirectoryInfo, CalafError>
        
    abstract tryReadXml:
        absolutePath: string -> Result<System.Xml.Linq.XElement, CalafError>
        
    abstract tryWriteXml:
        absolutePath: string * content: System.Xml.Linq.XElement -> Result<unit, CalafError>
