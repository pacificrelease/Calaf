// For more information see https://aka.ms/fsharp-console-apps

open Calaf

let rootPath = "../../../../.."
let workspace = Api.CreateWorkspace rootPath
let bumpableProject = Api.Workspace.getBumpableProjects workspace
let count = bumpableProject.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects are ready to bump 🚀. \n"