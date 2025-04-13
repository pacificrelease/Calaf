namespace Calaf

open System.IO
open Calaf.Errors

// Impure
module FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path
        
    let ReadFilesMatching (pattern: string) (path: DirectoryInfo) : FileInfo[] =
        path.GetFiles(pattern, SearchOption.AllDirectories)    

    let TryGetDirectoryInfo (path: string) : Result<DirectoryInfo, DomainError> =
        try
            if path |> getPathOrCurrentDir |> Directory.Exists
            then
                path
                |> getPathOrCurrentDir
                |> DirectoryInfo
                |> Ok
            else
                $"Path {path} does not exist or does not have enough permissions to access it."
                |> DomainError.InitWorkspaceError 
                |> Error
        with exn ->
            exn.Message
            |> DomainError.InitWorkspaceError
            |> Error

module Xml =        
    let TryLoadXml (absolutePath: string) : System.Xml.Linq.XElement option =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)
            Some xml
        with _ ->
            None

module Clock =
    let NowUtc () =
        System.DateTime.UtcNow