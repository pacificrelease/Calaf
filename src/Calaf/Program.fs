// For more information see https://aka.ms/fsharp-console-apps
// Composition Root

open System

open Calaf.Application.Workspace
open Calaf.Domain.DomainTypes
open Calaf.Infrastructure

let path = String.Empty
let timeStamp = Clock.now()
let result =
    getWorkspace
        FileSystem.tryReadWorkspace
        Git.tryReadRepository
        path
        timeStamp

match result with
| Error error ->
    printfn $"{error}"
    Environment.Exit(1)
    
| Ok workspace ->
    match workspace.Repository, workspace.Suite with
    | Some _, Suite.Empty ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository found. Skipping now..."
        printfn "Suite is empty. No projects found."
        printfn "Please add suitable projects to the workspace."
        Environment.Exit(1)
        
    | None, Suite.Empty ->
        printfn $"Workspace: {workspace.Directory}."
        printfn "Git repository not found."
        printfn "Suite is empty. No projects found."
        printfn "Please add suitable projects to the workspace."
        Environment.Exit(1)
        
    | Some _, Suite.Set set ->
        match set.Version with
        | Some version ->
            printfn $"Workspace: {workspace.Directory}."
            printfn "Git repository found. Skipping now..."
            printfn $"Current Suite version is {version}. 🚀. \n"
            Environment.Exit(0)
            
        | None ->
            printfn $"Workspace: {workspace.Directory}."
            printfn "Git repository found. Skipping now..."
            printfn "Suite calendar version not found."
            printfn "Please init and add a calendar version's Version element to the PropertyGroup of the every projects."
            printfn "For example: <Version>2023.10</Version> \n"
            Environment.Exit(1)
            
    | None, Suite.Set set ->
        match set.Version with
        | Some version ->
            printfn $"Workspace: {workspace.Directory}."
            printfn "Git repository not found."
            printfn $"Current Suite version is {version}. 🚀. \n"
            Environment.Exit(0)
            
        | None ->
            printfn $"Workspace: {workspace.Directory}."
            printfn "Git repository not found."
            printfn "Suite calendar version not found."
            printfn "Please init and add a calendar version's Version element to the PropertyGroup of the every projects."
            printfn "For example: <Version>2023.10</Version> \n"
            Environment.Exit(1)