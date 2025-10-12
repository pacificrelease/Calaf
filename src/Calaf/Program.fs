open System
open Calaf.Application
open Calaf.Infrastructure

[<Literal>]
let private loadTenTags = 10uy
[<Literal>]
let private changelogFileName = "CHANGELOG.md"

[<EntryPoint>]
let main args =
    let path     = String.Empty
    let deps     = MakeContext.createDeps
    let console  = MakeContext.createConsole
    let settings = MakeSettings.tryCreate loadTenTags changelogFileName
    Make.run path args deps console settings