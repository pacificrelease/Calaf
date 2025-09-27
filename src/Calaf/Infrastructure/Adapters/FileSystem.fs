namespace Calaf.Infrastructure

open Calaf.Contracts
open Calaf.Application

module internal FileSystemGateway =
    let toFileInfo
        (file: System.IO.FileInfo) =
        { Name = file.Name
          Directory = file.DirectoryName
          Extension = file.Extension
          AbsolutePath = file.FullName
          Exists = file.Exists }
        
    let toProjectXmlFileInfo
        (file: System.IO.FileInfo)
        (xml: System.Xml.Linq.XElement) : ProjectXmlFileInfo =        
        { Info = toFileInfo file
          Content = xml }
        
    let toWorkspaceDirectoryInfo
        (directoryInfo: System.IO.DirectoryInfo)
        (changelog: System.IO.FileInfo)
        (projects: (System.IO.FileInfo * System.Xml.Linq.XElement) seq) : DirectoryInfo =        
        { Directory = directoryInfo.FullName
          Changelog = toFileInfo changelog
          Projects = projects
            |> Seq.map (fun (fileInfo, xml) -> toProjectXmlFileInfo fileInfo xml)
            |> Seq.toList }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal FileSystem =    
    module Markdown =
        let private utf8NoBom = System.Text.UTF8Encoding(false, true)        
        let private normalizeLF (content: string) =
            content.Replace("\r\n", "\n").Replace("\r", "\n")
        let private directory (absolutePath: string) =
            let dir =
                match System.IO.Path.GetDirectoryName absolutePath with
                | null | "" -> System.IO.Directory.GetCurrentDirectory()
                | d -> d
            if not (System.String.IsNullOrWhiteSpace dir) then
                System.IO.Directory.CreateDirectory dir |> ignore
            dir
        
        let load
            (absolutePath: string) : Result<string option, InfrastructureError> =
            try
                if not (System.IO.File.Exists absolutePath)
                then Ok None
                else
                    System.IO.File.ReadAllText(absolutePath, utf8NoBom) |> Some |> Ok
            with
            | :? System.IO.FileNotFoundException
            | :? System.IO.DirectoryNotFoundException ->
                // The file may disappear between the check and the read
                Ok None
            | :? System.UnauthorizedAccessException as exn ->
                (absolutePath, exn :> System.Exception)
                |> FileAccessDenied
                |> FileSystem
                |> Error            
            | exn ->
                (absolutePath, exn)
                |> MarkdownLoadFailed
                |> FileSystem
                |> Error                
                                
        let write
            (absolutePath: string)
            (content: string) : Result<unit, InfrastructureError> =            
            try
                absolutePath
                |> System.IO.Path.GetDirectoryName
                |> System.IO.Directory.CreateDirectory
                |> ignore
                
                let directory = directory absolutePath                
                let tempFile = System.IO.Path.Combine(directory, System.IO.Path.GetRandomFileName())

                try
                    do
                        let content = normalizeLF content
                        System.IO.File.WriteAllText(tempFile, content, utf8NoBom)

                    if System.IO.File.Exists absolutePath
                    then
                        use src = new System.IO.FileStream(absolutePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)
                        use dst = new System.IO.FileStream(tempFile, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.None)
                        src.CopyTo(dst)                    
                    if System.IO.File.Exists absolutePath
                    then System.IO.File.Move(tempFile, absolutePath, overwrite = true)
                    else System.IO.File.Move(tempFile, absolutePath)
                with
                | exn ->
                    if System.IO.File.Exists tempFile
                    then System.IO.File.Delete tempFile
                    reraise ()
                    
                Ok()
            with
            | :? System.UnauthorizedAccessException as exn ->
                (absolutePath, exn :> System.Exception)
                |> FileAccessDenied
                |> FileSystem
                |> Error
            | exn ->
                (absolutePath, exn)
                |> MarkdownSaveFailed
                |> FileSystem
                |> Error
        
    module Xml =
        let load
            (absolutePath: string) : Result<System.Xml.Linq.XElement, InfrastructureError> =
            try
                let settings = System.Xml.XmlReaderSettings()
                settings.DtdProcessing <- System.Xml.DtdProcessing.Prohibit
                settings.XmlResolver <- null
                use reader = System.Xml.XmlReader.Create(absolutePath, settings)
                let xml = System.Xml.Linq.XElement.Load(reader)            
                xml |> Ok
            with
            | :? System.UnauthorizedAccessException as exn ->
                (absolutePath, exn :> System.Exception)
                |> FileAccessDenied
                |> FileSystem
                |> Error
            | exn ->
                (absolutePath, exn)
                |> XmlLoadFailed
                |> FileSystem
                |> Error

        let save
            (absolutePath: string)
            (content: System.Xml.Linq.XElement) : Result<unit, InfrastructureError> =
            try        
                let options = System.Xml.Linq.SaveOptions.None
                content.Save(absolutePath, options) |> Ok
            with
            | :? System.UnauthorizedAccessException as exn ->
                (absolutePath, exn :> System.Exception)
                |> FileAccessDenied
                |> FileSystem
                |> Error
            | exn ->
                (absolutePath, exn)
                |> XmlSaveFailed
                |> FileSystem
                |> Error
                
    module Directory =        
        open FsToolkit.ErrorHandling
        open System.IO
        
        open Calaf.Extensions.InternalExtensions
        
        let private getFile
            (directory: DirectoryInfo)
            (filename: string) =
            try
                let absolute = Path.GetFullPath (filename, directory.FullName)
                FileInfo absolute |> Ok
            with exn ->
                exn
                |> FileGetFailed
                |> FileSystem
                |> Error
                
        let private getXmlFile
            (fileInfo: FileInfo) =
            result {
                let! xml = Xml.load fileInfo.FullName
                return fileInfo, xml
            }

        let private listFiles
            (directory: DirectoryInfo)
            (pattern: string) =
            try
                directory.GetFiles(pattern, SearchOption.AllDirectories)
                |> Array.toList
                |> Ok
            with exn ->
                exn
                |> FilesScanFailed
                |> FileSystem
                |> Error        

        let private info
            (path: string) =
            try
                let path = DirectoryInfo path
                if path.Exists
                then path |> Ok
                else DirectoryDoesNotExist
                     |> FileSystem
                     |> Error
            with exn ->
                exn
                |> DirectoryAccessDenied
                |> FileSystem
                |> Error        
                
        let workspace
            (path: string)
            (pattern: string)
            (changelogFilename: string)=
            result {
                let! dirInfo = info path                
                let! files = listFiles dirInfo pattern                
                let projects, _ =
                    files
                   |> List.map getXmlFile
                   |> Result.partition
                let! changelog = getFile dirInfo changelogFilename
                return FileSystemGateway.toWorkspaceDirectoryInfo dirInfo changelog projects
            }
            
type FileSystem() =
    interface IFileSystem with
        member _.tryReadDirectory directory pattern changelogFilename =
            FileSystem.Directory.workspace directory pattern changelogFilename
            |> Result.mapError CalafError.Infrastructure        
            
        member _.tryReadXml absolutePath =
            FileSystem.Xml.load absolutePath
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryReadMarkdown absolutePath=
            FileSystem.Markdown.load absolutePath
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryWriteXml (absolutePath, content) =
            FileSystem.Xml.save absolutePath content
            |> Result.mapError CalafError.Infrastructure
            |> Result.map ignore
            
        member _.tryWriteMarkdown (absolutePath, content) =
            FileSystem.Markdown.write absolutePath content
            |> Result.mapError CalafError.Infrastructure
            |> Result.map ignore