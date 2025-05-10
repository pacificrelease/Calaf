// Impure
namespace Calaf.Infrastructure

open System.IO
open FsToolkit.ErrorHandling

open Calaf.Extensions.InternalExtensions

module internal Xml =        
    let tryLoadXml (absolutePath: string) : Result<System.Xml.Linq.XElement, InfrastructureError> =
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
            
    
    let trySaveXml (absolutePath: string) (xml: System.Xml.Linq.XElement) : Result<System.Xml.Linq.XElement, InfrastructureError> =
        try            
            let options = System.Xml.Linq.SaveOptions.None
            xml.Save(absolutePath, options)
            xml |> Ok
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

module internal FileSystem =     
    let private tryScanFileInfos(path: DirectoryInfo) (pattern: string) =
        try
            path.GetFiles(pattern, SearchOption.AllDirectories) |> Ok
        with exn ->
            exn
            |> FilesScanFailed
            |> FileSystem
            |> Error
        

    let private tryGetDirectoryInfo (path: string) =
        try
            let path = DirectoryInfo path
            if path.Exists
            then path |> Ok
            else DirectoryDoesNotExist |> FileSystem |> Error
        with exn ->
            exn
            |> DirectoryAccessDenied
            |> FileSystem
            |> Error
            
    let tryReadWorkspace (path: string) (pattern: string) =
        result {
            let! dirInfo = tryGetDirectoryInfo path
            let! files = tryScanFileInfos dirInfo pattern
            let projects, errors = files
                                   |> Array.map (fun fileInfo -> Xml.tryLoadXml fileInfo.FullName |> Result.map (fun xml ->  fileInfo, xml))
                                   |> Result.partition
                                   
            return Mappings.toWorkspaceDirectoryInfo dirInfo projects
        }