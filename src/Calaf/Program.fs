// For more information see https://aka.ms/fsharp-console-apps

open Calaf.FileSystem

let rootPath = Some "D:/"

let projects = ListProjects(None)
let count = projects.Length

printfn "Calendar Version has called. 🚀. \n"
printfn $"{count} projects found 🚀. \n"