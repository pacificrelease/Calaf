// For more information see https://aka.ms/fsharp-console-apps

open Calaf

//let rootPath = Some "D:/"
let rootPath = Some "../../../../.."
let workspace = Api.CreateWorkspace rootPath

let count = workspace.Projects.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects found 🚀. \n"