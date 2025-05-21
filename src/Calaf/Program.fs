// For more information see https://aka.ms/fsharp-console-apps
// Composition Root

open System

open Calaf.Application.Workspace
open Calaf.Domain.DomainTypes.Entities
open Calaf.Infrastructure

let path = String.Empty
let git = Calaf.Infrastructure.Git()
let clock = Calaf.Infrastructure.Clock()
let result =
    getWorkspace
        git
        clock
        FileSystem.tryReadWorkspace        
        path

match result with
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