open System
open Calaf.Application
open Calaf.Infrastructure

[<EntryPoint>]
let main args =
    let path     = String.Empty
    let deps     = MakeContext.createDeps
    let console  = MakeContext.createConsole
    let config = MakeConfiguration.defaultValue
    Make.run path args deps config console