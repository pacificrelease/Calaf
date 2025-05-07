// Impure
namespace Calaf.Infrastructure

open System.IO

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
            exn :> System.Exception
            |> FileAccessDenied
            |> FileSystem
            |> Error
        | exn ->
            exn
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
            exn :> System.Exception
            |> FileAccessDenied
            |> FileSystem
            |> Error
        | exn ->
            exn
            |> XmlSaveFailed
            |> FileSystem
            |> Error

module internal FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path        
     
    let tryScanFiles(path: DirectoryInfo) (pattern: string) =
        try
            path.GetFiles(pattern, SearchOption.AllDirectories) |> Ok
        with exn ->
            exn
            |> FilesScanFailed
            |> FileSystem
            |> Error
        

    let tryGetDirectory (path: string) =
        try
            let path = path |> getPathOrCurrentDir |> DirectoryInfo
            if path.Exists
            then
                path |> Ok
            else
                DirectoryDoesNotExist
                |> FileSystem
                |> Error
        with exn ->
            exn
            |> DirectoryAccessDenied
            |> FileSystem
            |> Error