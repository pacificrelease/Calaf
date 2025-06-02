// For more information see https://aka.ms/fsharp-console-apps
// Composition Root
open System

open Calaf.Application
open Calaf.Infrastructure

module internal BumpWorkspace =
    open FsToolkit.ErrorHandling
    
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
    [<Literal>]
    let private loadTenTags = 1uy
    let private getPathOrCurrentDir path =        
        if String.IsNullOrWhiteSpace path then "." else path
        
    let run path =
        result {
            let path = path |> getPathOrCurrentDir
            let context = BumpContext.createDefault
            let! settings = BumpSettings.tryCreate supportedFilesPattern loadTenTags
            let! result = Bump.run path context settings
            return result
        }
        
module internal OutputWorkspace =
    let run result =
        let console = Console()
        Output.run result console
        
module internal ExitCode =
    let map result =
        match result with
        | Ok _ -> 0
        | Error _ -> 1

[<EntryPoint>]
let main args =
    
    let path = String.Empty        
    path
    |> BumpWorkspace.run
    |> OutputWorkspace.run
    |> ExitCode.map