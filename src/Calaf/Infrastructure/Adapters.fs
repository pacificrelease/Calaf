namespace Calaf.Infrastructure

open Calaf.Application

module internal File =
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
        (content: System.Xml.Linq.XElement) : Result<System.Xml.Linq.XElement, InfrastructureError> =
        try        
            let options = System.Xml.Linq.SaveOptions.None
            content.Save(absolutePath, options)
            content |> Ok
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
    
module internal Directory =
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
            
    let list
        (path: string)
        (pattern: string) =
        result {
            let! dirInfo = info path
            let! files = find dirInfo pattern
            let projects, _ =
                files
               |> List.map (fun fileInfo -> File.load fileInfo.FullName
                                            |> Result.map (fun xml ->  fileInfo, xml))
               |> Result.partition                                   
            return Mappings.toWorkspaceDirectoryInfo dirInfo projects
        }

type FileSystem() =
    interface IFileSystem with
        member _.tryReadDirectory directory pattern =
            Directory.list directory pattern
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryReadXml absolutePath =
            File.load absolutePath
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryWriteXml (absolutePath, content) =
            File.save absolutePath content
            |> Result.mapError CalafError.Infrastructure
            |> Result.map ignore
            
type Clock() =
    interface IClock with
        member _.now() =
            System.DateTimeOffset.UtcNow