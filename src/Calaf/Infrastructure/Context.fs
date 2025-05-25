namespace Calaf.Infrastructure

open Calaf.Application

module internal BumpContext =
    let create fileSystem git clock=
        { FileSystem = fileSystem
          Git = git
          Clock = clock }
        
    let createDefault =
        let fileSystem = new FileSystem() :> IFileSystem
        let git = new Git() :> IGit
        let clock = Clock() :> IClock
        create fileSystem git clock