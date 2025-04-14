// Impure
namespace Calaf

open System.IO

open Calaf.Domain.Errors

module internal FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path
        
     // TODO: Add Result/Error handling + try with
    let ReadFilesMatching (pattern: string) (path: DirectoryInfo) : FileInfo[] =
        path.GetFiles(pattern, SearchOption.AllDirectories)    

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

