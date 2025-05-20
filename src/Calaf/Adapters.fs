namespace Calaf.Infrastructure

open Calaf.Application

type Git() =
    interface IGit with
        member this.tryApply directory commitMessage tagName signature =
            failwith "todo"
            
        member this.tryReadRepository directory maxTagsToRead timeStamp =
            failwith "todo"


type FileSystem() =
    interface IFileSystem with
        member this.tryReadDirectory directory pattern =
            failwith "todo"
            
        member this.tryReadXml absolutePath =
            failwith "todo"
            
        member this.tryWriteXml absolutePath content =
            failwith "todo"