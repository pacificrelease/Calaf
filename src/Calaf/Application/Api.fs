namespace Calaf.Application

module Api =
    open FsToolkit.ErrorHandling
    
    open Calaf.Contracts
    
    [<Literal>]
    let private supportedFilesPattern = "*.?sproj"
    [<Literal>]
    let private tagsToLoad = 5uy
    
    let execute
        (command : Command, path: string, fileSystem: IFileSystem, git: IGit, clock: IClock) =
        result {
            match command with
            | Command.Make makeType ->
                let! makeSettings = 
                    MakeSettings.tryCreate supportedFilesPattern tagsToLoad
                let ctx = {
                    Directory = path
                    Type = makeType
                    Settings = makeSettings
                    FileSystem = fileSystem
                    Git = git
                    Clock = clock
                }
                let! summary = Make.run2 ctx
                return summary
        }