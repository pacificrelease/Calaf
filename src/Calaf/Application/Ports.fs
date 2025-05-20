namespace Calaf.Application

open Calaf.Contracts

type IGit =
    abstract tryReadRepository:
        directory: string -> maxTagsToRead: int -> timeStamp: System.DateTimeOffset -> Result<GitRepositoryInfo, CalafError>
        
    abstract tryApply:
        directory: string -> commitMessage: string -> tagName: string -> signature: GitSignatureInfo -> Result<unit, CalafError>


type IFileSystem =
    abstract tryReadDirectory:
        directory: string -> pattern: string -> Result<DirectoryInfo, CalafError>
        
    abstract tryReadXml:
        absolutePath: string -> Result<System.Xml.Linq.XElement, CalafError>
        
    abstract tryWriteXml:
        absolutePath: string -> content: System.Xml.Linq.XElement -> Result<unit, CalafError>