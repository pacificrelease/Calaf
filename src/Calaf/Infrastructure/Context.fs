namespace Calaf.Infrastructure

open Calaf.Application

module MakeContext =
    let private fs =
        Calaf.Infrastructure.FileSystem() :> IFileSystem
    let private git =
        Calaf.Infrastructure.Git() :> IGit
    let private clock =
        Clock() :> IClock
    
    let createConsole =        
        Console() :> IConsole
        
    let createDeps : Deps =
        { UtcNow = clock.utcNow
          TryReadDirectory = fs.tryReadDirectory
          TryWriteXml = fs.tryWriteXml
          TryWriteMarkdown = fs.tryWriteMarkdown
          TryGetRepo = git.tryGetRepo
          TryListCommits = git.tryListCommits
          TryCreateCommit = git.tryApply }