// For more information see https://aka.ms/fsharp-console-apps

open System
open Calaf

[<Literal>]
let searchFilesPattern = "*.?sproj"

let rootPath = "../../../../.."
let result = Api.CreateWorkspace rootPath searchFilesPattern

match result with
| Error error ->
    printfn $"{error}"
    Environment.Exit(1)
    
| Ok workspace ->
    let nextVersion = Clock.NowUtc |> Api.GetNextVersion workspace
    if workspace.Version.PropertyGroup.IsNone
    then
        printfn $"Workspace {workspace.Directory} not initialized. \n"
        printfn "Please init and add a calendar version to the property group of the projects. \n"
        printfn "For example: <Version>2023.10</Version> \n"
        Environment.Exit(1)
    else
        printfn $"Current version is {workspace.Version.PropertyGroup.Value}. 🚀. \n"
        printfn $"Next version will be {nextVersion.Value.PropertyGroup.Value}. 🚀. \n"
        Environment.Exit(0)