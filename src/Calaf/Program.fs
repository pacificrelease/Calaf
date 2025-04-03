// For more information see https://aka.ms/fsharp-console-apps

open Calaf.FileSystem

let projects = ListProjects(Some "../../..")
let count = projects.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects found 🚀. \n"