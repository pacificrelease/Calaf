// For more information see https://aka.ms/fsharp-console-apps

open System
open Calaf

let rootPath = "../../../../.."
let now = DateTime.UtcNow
let workspace = Api.CreateWorkspace null
let pendingProjectsCount = workspace.Projects
                        |> Project.choosePending
                        |> Array.length
let nextVersion = Api.GetNextVersion workspace now

if workspace.Version.PropertyGroup.IsNone
then
    printfn $"Workspace {workspace.Directory} not initialized. \n"
    printfn "Please init and add a calendar version to the property group of the projects. \n"
    printfn "For example: <Version>2023.10</Version> \n"
    Environment.Exit(1)
else
    printfn $"Current calendar version of property group is {workspace.Version.PropertyGroup}. 🚀. \n"
    printfn $"{pendingProjectsCount} projects are ready to bump 🚀. \n"
    printfn $"{nextVersion} will be a next version 🚀. \n"
    Environment.Exit(0)