namespace Calaf.Infrastructure

open Calaf.Application

module MakeContext =        
    let create =
        let fileSystem = Calaf.Infrastructure.FileSystem() :> IFileSystem
        let git = Git2() :> IGit
        let clock = Clock() :> IClock
        let console = Console() :> IConsole
        { FileSystem = fileSystem
          Git = git
          Clock = clock
          Console = console }