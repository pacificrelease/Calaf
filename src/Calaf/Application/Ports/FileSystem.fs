namespace Calaf.Application

open Calaf.Contracts

type IFileSystem =
    abstract tryReadDirectory:
        directory: string -> pattern: string -> Result<DirectoryInfo, CalafError>
        
    abstract tryReadXml:
        absolutePath: string -> Result<System.Xml.Linq.XElement, CalafError>
        
    abstract tryReadMarkdown:
        absolutePath: string -> Result<string option, CalafError>
        
    abstract tryWriteXml:
        absolutePath: string * content: System.Xml.Linq.XElement -> Result<unit, CalafError>
        
    abstract tryWriteMarkdown:
        absolutePath: string * salt: string * lines: string list -> Result<unit, CalafError>