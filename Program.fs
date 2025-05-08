// For more information see https://aka.ms/fsharp-console-apps

open System
open Calaf
open Calaf.Domain.DomainTypes

let result = Api.Workspace.create null

match result with
| Error error ->
    printfn $"{error}"
    Environment.Exit(1)
    
| Ok workspace ->
    match workspace.Repository, workspace.Suite.Version with
    | Some _, Some version ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository found. Skipping now..."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)
        
    | None, Some version ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository not found."
        printfn $"Current Suite version is {version}. 🚀. \n"
        Environment.Exit(0)
        
    | Some _, None ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository found. Skipping now..."
        printfn "Suite calendar version not found."
        printfn "Please init and add a calendar version to the Version element of the PropertyGroup to the projects."
        printfn "For example: <Version>2023.10</Version> \n"
        Environment.Exit(1)
        
    | None, None ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository not found."
        printfn "Suite calendar version not found."
        printfn "Please init and add a calendar version to the Version element of the PropertyGroup to the projects."
        printfn "For example: <Version>2023.10</Version> \n"
        Environment.Exit(1)