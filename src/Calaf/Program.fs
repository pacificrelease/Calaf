// For more information see https://aka.ms/fsharp-console-apps
// Composition Root
open System
open Calaf.Application
open Calaf.Infrastructure
        
[<Literal>]
let private supportedFilesPattern = "*.?sproj"
[<Literal>]
let private loadTenTags = 10uy

[<EntryPoint>]
let main args =
    let path     = String.Empty
    let context  = MakeContext.create
    let settings = MakeSettings.tryCreate supportedFilesPattern loadTenTags
    Make.run path args context settings