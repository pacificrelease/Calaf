// For more information see https://aka.ms/fsharp-console-apps
// Composition Root

open System

open Calaf.Application.Workspace
open Calaf.Domain.DomainTypes
open Calaf.Infrastructure

let path = String.Empty
let timeStamp = Clock.now()
let result =
    bumpWorkspace
        FileSystem.tryReadWorkspace
        Git.tryReadRepository
        Clock.now
        path

match result with
| Error error ->
    printfn $"{error}"
    Environment.Exit(1)
    
| Ok workspace ->
    match workspace.Repository, workspace.Suite with
    | Some _, Suite.StandardSet { Version = version; Projects = _ } ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository found. Skipping now..."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)
            
    | None, Suite.StandardSet { Version = version; Projects = _ } ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository not found."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)