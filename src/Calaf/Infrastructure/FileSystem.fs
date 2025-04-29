// Impure
namespace Calaf.Infrastructure

open System.IO

open Calaf.Domain.Errors

module internal FileSystem =
    let private getPathOrCurrentDir path =        
        if System.String.IsNullOrWhiteSpace path then "." else path        
     
    let tryReadFiles (pattern: string) (path: DirectoryInfo) =
        try
            path.GetFiles(pattern, SearchOption.AllDirectories)
            |> Ok
        with exn ->
            exn
            |> ReadProjectsError
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
                |> NotExistOrBadPath
                |> FileSystem
                |> Error
        with exn ->
            exn
            |> AccessPathError
            |> FileSystem
            |> Error