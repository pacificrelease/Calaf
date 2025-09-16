open System
open Calaf.Application
open Calaf.Infrastructure
        
[<Literal>]
let private supportedFilesPattern = "*.?sproj"
[<Literal>]
let private loadTenTags = 10uy
[<Literal>]
let private changelogFileName = "CHANGELOG.md"

[<EntryPoint>]
let main args =
    let path     = String.Empty
    let context  = MakeContext.create
    let settings = MakeSettings.tryCreate supportedFilesPattern loadTenTags changelogFileName
    Make.run path args context settings