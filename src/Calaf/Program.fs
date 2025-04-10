// For more information see https://aka.ms/fsharp-console-apps

open Calaf

let rootPath = "../../../../.."
let workspace = Api.CreateWorkspace rootPath
let incrementableProject = Api.Workspace.getIncrementableProjects workspace
let count = incrementableProject.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects are ready to bump 🚀. \n"