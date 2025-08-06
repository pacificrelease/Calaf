namespace Calaf.Infrastructure

open Calaf.Application

module MakeContext =        
    let create =
        let fileSystem = Calaf.Infrastructure.FileSystem() :> IFileSystem
        let git = Calaf.Infrastructure.Git() :> IGit
        let clock = Clock() :> IClock
        let console = Console() :> IConsole
        { FileSystem = fileSystem
          Git = git
          Clock = clock
          Console = console }