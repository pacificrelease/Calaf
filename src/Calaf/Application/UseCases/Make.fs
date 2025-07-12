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
        let ok (console: IConsole) (o: obj) =
            match o with
            | :? Workspace as workspace ->
                match workspace.Repository, workspace.Suite with
                | Some _, Suite.StandardSet (version, _ ) ->
                    console.write $"Workspace: {workspace.Directory}."
                    console.write "Git repository found. Skipping now..."
                    console.success $"Current Suite version is {version}. 🚀. \n"                    
                | None, Suite.StandardSet (version, _) ->
                    console.write $"Workspace: {workspace.Directory}."
                    console.write "Git repository not found."
                    console.success $"Current Suite version is {version}. 🚀. \n"
            | _ -> ()            
                    
        let error (console: IConsole) e =            
            console.error $"{e}"    
    
    // TODO: Remove this type after refactoring
    let private nightly path (context: MakeContext) settings =
        result {
            let timeStamp = context.Clock.now()
            let! dayOfMonth = timeStamp |> DateSteward.tryCreateDayOfMonth |> Result.mapError CalafError.Domain
            let! monthStamp = timeStamp |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes timeStamp                
            let! workspace,  _ = Workspace.tryCapture (dir, repo)                        |> Result.mapError CalafError.Domain
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
            let timeStamp = dependencies.Clock.now()
            let! dayOfMonth = timeStamp |> DateSteward.tryCreateDayOfMonth |> Result.mapError CalafError.Domain
            let! monthStamp = timeStamp |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr                
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryRead dependencies.Directory tagCount Version.versionPrefixes timeStamp                
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
            let timeStamp = context.Clock.now()            
            let! monthStamp = timeStamp |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes timeStamp                
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
            let timeStamp = dependencies.Clock.now()            
            let! monthStamp = timeStamp |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr                
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryRead dependencies.Directory tagCount Version.versionPrefixes timeStamp                
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
                    | MakeType.Stable ->
                        return! stable path context settings
            }
        let path = directory path
        let result = apply path arguments context settings
        result |> output context.Console |> exit