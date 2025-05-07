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
        with exn ->
            exn
            |> LoadFailure
            |> Xml
            |> Error
            
    
    let trySaveXml (absolutePath: string) (xml: System.Xml.Linq.XElement) =
        try            
            let options = System.Xml.Linq.SaveOptions.None
            xml.Save(absolutePath, options)
            xml
            |> Ok
        with exn ->
            exn
            |> SaveFailure
            |> Xml
            |> Error

module internal FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path        
     
    let tryReadFiles(path: DirectoryInfo) (pattern: string) =
        try
            path.GetFiles(pattern, SearchOption.AllDirectories)
            |> Ok
        with exn ->
            exn
            |> ReadFailure
            |> FileSystem
            |> Error
        

    let tryGetDirectory (path: string) =
        try
            let path = path |> getPathOrCurrentDir |> DirectoryInfo
            if path.Exists
            then
                path
                |> Ok
            else
                $"Path {path.FullName} does not exist or can't determine if it exists."
                |> NoPath
                |> FileSystem
                |> Error
        with exn ->
            exn
            |> AccessDenied
            |> FileSystem
            |> Error