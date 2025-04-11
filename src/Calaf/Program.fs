// For more information see https://aka.ms/fsharp-console-apps

open System
open Calaf

let rootPath = "../../../../.."
let now = DateTime.UtcNow
let workspace = Api.CreateWorkspace null
let pendingProjectsCount = workspace.Projects
                        |> Api.Project.choosePending
                        |> Array.length
let nextVersion = Api.GetNextVersion workspace now

printfn $"Current calendar version of property group is {workspace.Version.PropertyGroup}. 🚀. \n"
printfn $"{pendingProjectsCount} projects are ready to bump 🚀. \n"
printfn $"{nextVersion} will be a next version 🚀. \n"