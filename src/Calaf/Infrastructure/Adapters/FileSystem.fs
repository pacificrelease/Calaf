namespace Calaf.Infrastructure

open Calaf.Contracts
open Calaf.Application

module internal FileSystemGateway =
    let toProjectXmlFileInfo
        (file: System.IO.FileInfo)
        (xml: System.Xml.Linq.XElement) : ProjectXmlFileInfo =
        { Name = file.Name
          Directory = file.DirectoryName
          Extension = file.Extension
          AbsolutePath = file.FullName
          Content = xml }
        
    let toWorkspaceDirectoryInfo
        (directoryInfo: System.IO.DirectoryInfo)
        (projects: (System.IO.FileInfo * System.Xml.Linq.XElement) seq) : DirectoryInfo =        
        { Directory = directoryInfo.FullName
          Projects = projects
            |> Seq.map (fun (fileInfo, xml) -> toProjectXmlFileInfo fileInfo xml)
            |> Seq.toList }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal FileSystem =    
    module Markdown =
        let private utf8NoBom = System.Text.UTF8Encoding(false, true)
        
        let private normalizeLF (text: string) =
            text.Replace("\r\n", "\n").Replace("\r", "\n")
        
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
                                
        let append
            (absolutePath: string)
            (content: string) : Result<unit, InfrastructureError> =
            try
                absolutePath
                |> System.IO.Path.GetDirectoryName
                |> System.IO.Directory.CreateDirectory
                |> ignore
                
                System.IO.File.AppendAllText(absolutePath, content, utf8NoBom) |> Ok
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
                
        let append2
            (absolutePath: string)
            (salt: string)
            (lines: string list) : Result<unit, InfrastructureError> =
            try
                let absolutePath = System.IO.Path.GetFullPath(absolutePath)
                
                let contentLF =
                    match lines with
                    | [] -> System.String.Empty
                    | _  -> System.String.Join("\n", lines)
                    
                let file =
                    if System.IO.File.Exists absolutePath
                    then System.IO.File.ReadAllText(absolutePath)
                    else System.String.Empty
                    
                let eol =
                    if file.Contains("\r\n")
                    then "\r\n"
                    else "\n"                
                let fileLF = normalizeLF file
                
                let combinedLF =                
                    if System.String.IsNullOrEmpty fileLF
                    then contentLF
                    else contentLF + fileLF
                    
                let newContent =
                    if eol = "\n"
                    then combinedLF
                    else combinedLF.Replace("\n", "\r\n")
                    
                let directory = System.IO.Path.GetDirectoryName absolutePath
                if not (System.String.IsNullOrEmpty directory)
                then System.IO.Directory.CreateDirectory directory |> ignore
                
                let tempFileName = $".{salt}.tmp"
                let tmpAbsolutePath = System.IO.Path.Combine(directory, tempFileName)
                
                System.IO.File.WriteAllText(tmpAbsolutePath, newContent, utf8NoBom)
                
                if System.IO.File.Exists absolutePath
                then
                    System.IO.File.Replace(tmpAbsolutePath, absolutePath, null, true) |> Ok
                else
                    System.IO.File.Move(tmpAbsolutePath, absolutePath, true) |> Ok
            with
            | :? System.UnauthorizedAccessException as exn ->
                (absolutePath, exn :> System.Exception)
                |> FileAccessDenied
                |> FileSystem
                |> Error
            | exn ->
                System.IO.File.Delete absolutePath
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

        let private find
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
                
        let private load
            (fileInfo: FileInfo) =
            result {
                let! xml = Xml.load fileInfo.FullName
                return fileInfo, xml
            }
                
        let list
            (path: string)
            (pattern: string) =
            result {
                let! dirInfo = info path
                let! files = find dirInfo pattern
                let projects, _ =
                    files
                   |> List.map load
                   |> Result.partition                                   
                return FileSystemGateway.toWorkspaceDirectoryInfo dirInfo projects
            }
            
type FileSystem() =
    interface IFileSystem with
        member _.tryReadDirectory directory pattern =
            FileSystem.Directory.list directory pattern
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
            
        member _.tryWriteMarkdown (absolutePath, salt, lines) =
            FileSystem.Markdown.append2 absolutePath salt lines