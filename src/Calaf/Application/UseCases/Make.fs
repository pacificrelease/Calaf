namespace Calaf.Application

open System
open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes.Entities
open Calaf.Application

module internal Make =
    module private Arguments =
        let read (console: IConsole) (arguments: string[]) =
            console.read arguments
        
    module private Console =
        let ok (console: IConsole) (workspace: Workspace) =
            console.write $"Processing workspace at: {workspace.Directory}."            
            match workspace.Repository with
            | Some (Repository.Ready _) ->
                console.write "Git repository is ready. A version tag and commit created."                
            | Some (Repository.Dirty _) ->
                console.write "Git repository has uncommitted changes. A version tag and commit created, but local changes was not included."
            | Some _  ->
                console.write "Git repository is not in a clean state. Version tagging skipped. Please fix the repository state to enable version tagging."                
            | None ->
                console.write "No Git repository found. Version tagging skipped."
                
            let versionString = Version.toString workspace.Version
            console.success $"Version applied: {versionString}"
                    
        let error (console: IConsole) e =            
            console.error $"{e}"
            
    // TODO: Remove this type after refactoring
    let private beta path (context: MakeContext) settings =
        result {
            let dateTimeOffset = context.Clock.utcNow()
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes dateTimeOffset                
            let! workspace,  _ = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! workspace', _ =
                Version.beta workspace.Version dateTimeOffset
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> context.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    context.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return workspace'
        }
    
    // TODO: Remove this type after refactoring
    let private nightly path (context: MakeContext) settings =
        result {
            let dateTimeOffset = context.Clock.utcNow()
            let! dayOfMonth = dateTimeOffset |> DateSteward.tryCreateDayOfMonth |> Result.mapError CalafError.Domain
            let! monthStamp = dateTimeOffset |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes dateTimeOffset                
            let! workspace,  _ = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! workspace', _ =
                Version.nightly workspace.Version (dayOfMonth, monthStamp)
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> context.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    context.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return workspace'
        }
        
    let private nightly2
        (dependencies: {| Directory: string; Settings: MakeSettings; FileSystem: IFileSystem; Git: IGit; Clock: IClock |}) =
        result {
            let dateTimeOffset = dependencies.Clock.utcNow()
            let! dayOfMonth = dateTimeOffset |> DateSteward.tryCreateDayOfMonth |> Result.mapError CalafError.Domain
            let! monthStamp = dateTimeOffset |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr                
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryRead dependencies.Directory tagCount Version.versionPrefixes dateTimeOffset                
            let! workspace, captureEvents =
                Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! workspace', releaseEvents =
                Version.nightly workspace.Version (dayOfMonth, monthStamp)
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> dependencies.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    dependencies.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return (workspace', captureEvents @ releaseEvents)
        }
        
    // TODO: Remove this type after refactoring
    let private stable path (context: MakeContext) settings =
        result {
            let dateTimeOffset = context.Clock.utcNow()            
            let! monthStamp = dateTimeOffset |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes dateTimeOffset                
            let! workspace,  _ = Workspace.tryCapture (dir, repo)          |> Result.mapError CalafError.Domain
            let! workspace', _ =
                Version.stable workspace.Version monthStamp
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain                
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> context.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    context.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return workspace'
        }
        
    let private stable2
        (dependencies: {| Directory: string; Settings: MakeSettings; FileSystem: IFileSystem; Git: IGit; Clock: IClock |}) =
        result {
            let dateTimeOffset = dependencies.Clock.utcNow()            
            let! monthStamp = dateTimeOffset |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr                
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryRead dependencies.Directory tagCount Version.versionPrefixes dateTimeOffset                
            let! workspace, captureEvents = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! workspace', releaseEvents =
                Version.stable workspace.Version monthStamp
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain                
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> dependencies.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    dependencies.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return (workspace', captureEvents @ releaseEvents)
        }
    
    let private directory path =        
        if String.IsNullOrWhiteSpace path then "." else path
        
    let private input (console: IConsole) (arguments: string[]) =
        Arguments.read console arguments
        
    let private output (console: IConsole) result =
        match result with
        | Error error ->
            Console.error console error
            result
        | Ok workspace ->
            Console.ok console workspace
            result
            
    let private exit result =
        match result with
        | Ok    _ -> 0
        | Error _ -> 1
        
    let run2 context =
        let dependencies = {|
            Directory = context.Directory
            Settings = context.Settings
            FileSystem = context.FileSystem
            Git = context.Git
            Clock = context.Clock
        |}
        match context.Type with
        | MakeType.Stable  -> stable2 dependencies
        | MakeType.Beta    -> failwith "not implemented yet"
        | MakeType.Nightly -> nightly2 dependencies      
        
    let run path arguments context settings  =
        let apply path arguments context settings =
            result {
                let! settings = settings
                let! cmd = input context.Console arguments
                match cmd with
                | Command.Make strategy ->
                    match strategy with
                    | MakeType.Nightly ->
                        return! nightly path context settings
                    | MakeType.Beta ->
                        return! beta path context settings
                    | MakeType.Stable ->
                        return! stable path context settings
            }
        let path = directory path
        let result = apply path arguments context settings
        result |> output context.Console |> exit