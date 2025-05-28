namespace Calaf.Infrastructure

open Calaf.Application

module BumpContext =
    let create fileSystem git clock=
        { FileSystem = fileSystem
          Git = git
          Clock = clock }
        
    let createDefault =
        let fileSystem = Calaf.Infrastructure.FileSystem() :> IFileSystem
        let git = Calaf.Infrastructure.Git() :> IGit
        let clock = Clock() :> IClock
        create fileSystem git clock
        
module OutputContext =
    let create console =
        { Console = console }
        
    let createDefault =
        let console = Console() :> IConsole
        create console