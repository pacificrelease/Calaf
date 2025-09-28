namespace Calaf.Application

module Api =
    open FsToolkit.ErrorHandling
    
    open Calaf.Contracts    
    
    let private supportedFilesPatterns = [ "*.?sproj" ]
    [<Literal>]
    let private tagsToLoad = 5uy
    [<Literal>]
    let private changelogFileName = "CHANGELOG.md"
    
    let execute
        (command : Command, path: string, fileSystem: IFileSystem, git: IGit, clock: IClock) =
        result {
            match command with
            | Command.Make { Type = makeType } ->
                let! makeSettings = 
                    MakeSettings.tryCreate
                        supportedFilesPatterns
                        tagsToLoad
                        changelogFileName
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