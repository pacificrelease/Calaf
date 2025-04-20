// Impure
namespace Calaf

open System.IO

open Calaf.Domain.Errors

module internal FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path        
     
    let TryReadPatternFiles (pattern: string) (path: DirectoryInfo) : Result<FileInfo[], FileSystemError> =
        try
            path.GetFiles(pattern, SearchOption.AllDirectories)
            |> Ok
        with exn ->
            exn
            |> ReadProjectsError
            |> Error
        

    let TryGetDirectoryInfo (path: string) : Result<DirectoryInfo, FileSystemError> =
        try
            let path = path |> getPathOrCurrentDir |> DirectoryInfo
            if path.Exists
            then
                path
                |> Ok
            else
                $"Path {path.FullName} does not exist or can't determine if it exists."
                |> NotExistOrBadPath 
                |> Error
        with exn ->
            exn
            |> AccessPathError
            |> Error

