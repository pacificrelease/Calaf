// For more information see https://aka.ms/fsharp-console-apps

open Calaf

let rootPath = "../../../../.."
let workspace = Api.CreateWorkspace null
let count = workspace
            |> Api.Workspace.getBumpableProjects
            |> Array.length 

printfn $"Current calendar version of property group is {workspace.Version.PropertyGroup}. 🚀. \n"
printfn $"{count} projects are ready to bump 🚀. \n"