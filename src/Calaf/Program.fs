// For more information see https://aka.ms/fsharp-console-apps
// Composition Root

open System

open Calaf.Domain.DomainTypes.Entities

module internal BumpWorkspace =
    open FsToolkit.ErrorHandling
    
    open Calaf.Application
    open Calaf.Infrastructure
    
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

let path = String.Empty
match BumpWorkspace.run path with
| Error error ->
    printfn $"{error}"
    Environment.Exit(1)
    
| Ok workspace ->
    match workspace.Repository, workspace.Suite with
    | Some _, Suite.StandardSet (version, _ ) ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository found. Skipping now..."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)
            
    | None, Suite.StandardSet (version, _) ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository not found."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)