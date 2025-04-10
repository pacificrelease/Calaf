// For more information see https://aka.ms/fsharp-console-apps

open Calaf

let rootPath = "../../../../.."
let workspace = Api.CreateWorkspace rootPath
let bumpableProjects = Api.Workspace.getBumpableProjects workspace
let count = bumpableProjects.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects are ready to bump 🚀. \n"