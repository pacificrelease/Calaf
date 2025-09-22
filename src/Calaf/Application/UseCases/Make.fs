namespace Calaf.Application

open System
open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes
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
            
    let private tryReadCommits
        (workspace: Workspace)
        (git : IGit) = 
            match workspace.Repository with
            | Some (Dirty (_, { Version = Some { TagName = tagName; Version = CalVer _ } }))                    
            | Some (Ready (_, { Version = Some { TagName = tagName; Version = CalVer _ } })) ->                   
                git.tryListCommits workspace.Directory (Some tagName)
                |> Result.map (fun gci -> gci |> List.map Commit.create)
                |> Result.map Some
            | Some (Dirty (_, { Version = None }))
            | Some (Ready (_, { Version = None })) -> 
                git.tryListCommits workspace.Directory None                    
                |> Result.map (fun gci -> gci |> List.map Commit.create)
                |> Result.map Some
            | _ -> Ok None
            
    let private tryChangeset        
        commits
        nextVersion
        timeStamp =
        match commits with
        | Some commits -> ReleaseNotes.tryCreate commits nextVersion timeStamp
        | None -> None
        
    let private tryMake
        (path: string)
        (context: MakeContext)
        (settings: MakeSettings)        
        (make: CalendarVersion -> DateTimeOffset -> Result<CalendarVersion, DomainError>)=
        result {
            let dateTimeOffset = context.Clock.utcNow()
            let (DotNetXmlFilePattern searchPattern) = settings.ProjectsSearchPattern
            let (ChangelogFileName changelogFileName) = settings.ChangelogFileName
            let! dir = context.FileSystem.tryReadDirectory path searchPattern changelogFileName                 
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryGetRepo path tagCount Version.versionPrefixes List.Empty dateTimeOffset            
            let! workspace, _ =
                Workspace.tryCapture (dir, repo)
                |> Result.mapError CalafError.Domain                
            let! nextVersion =
                make workspace.Version dateTimeOffset
                |> Result.mapError CalafError.Domain
            let! releaseNotes, _ =
                tryReadCommits workspace context.Git
                |> Result.map (fun commits -> tryChangeset commits nextVersion dateTimeOffset)
                |> Result.map (Option.map (fun (cs, events) ->
                    Some cs, Some events) >> Option.defaultValue (None, None))
            let! workspace', _ =
                nextVersion
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain
            let snapshot =
                Workspace.snapshot workspace' releaseNotes
            do! snapshot.Projects
                |> List.traverseResultM (fun s -> context.FileSystem.tryWriteXml (s.AbsolutePath, s.Content))
                |> Result.map ignore
            do! snapshot.Changelog    
                |> Option.map (fun s ->
                    context.FileSystem.tryWriteMarkdown (s.AbsolutePath, s.ReleaseNotesContent)
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                
            do! snapshot.Repository
                |> Option.map (fun s ->
                    context.Git.tryApply (s.Directory, s.PendingFilesPaths) s.CommitText s.TagName
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())
            return workspace'
        }    
        
    let private tryNightly2
        (dependencies: {| Directory: string; Settings: MakeSettings; FileSystem: IFileSystem; Git: IGit; Clock: IClock |}) =
        result {
            let dateTimeOffset = dependencies.Clock.utcNow()
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let (ChangelogFileName changelogFileName) = dependencies.Settings.ChangelogFileName
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr changelogFileName                
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryGetRepo dependencies.Directory tagCount Version.versionPrefixes List.Empty dateTimeOffset                
            let! workspace, captureEvents =
                Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! nextVersion =
                Version.tryNightly workspace.Version dateTimeOffset
                |> Result.mapError CalafError.Domain
            let! releaseNotes, _ =
                tryReadCommits workspace dependencies.Git
                |> Result.map (fun commits -> tryChangeset commits nextVersion dateTimeOffset)
                |> Result.map (Option.map (fun (cs, events) ->
                    Some cs, Some events) >> Option.defaultValue (None, None))            
            let! workspace', releaseEvents =
                nextVersion
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain
            let snapshot = Workspace.snapshot workspace' releaseNotes
            do! snapshot.Projects
                |> List.traverseResultM (fun p -> dependencies.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! snapshot.Repository
                |> Option.map (fun p ->
                    dependencies.Git.tryApply (p.Directory, p.PendingFilesPaths) p.CommitText p.TagName
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())                                
            return (workspace', captureEvents @ releaseEvents)
        }
        
    let private tryStable2
        (dependencies: {| Directory: string; Settings: MakeSettings; FileSystem: IFileSystem; Git: IGit; Clock: IClock |}) =
        result {
            let dateTimeOffset = dependencies.Clock.utcNow()
            let (DotNetXmlFilePattern searchPatternStr) = dependencies.Settings.ProjectsSearchPattern
            let (ChangelogFileName changelogFileName) = dependencies.Settings.ChangelogFileName
            let! dir = dependencies.FileSystem.tryReadDirectory dependencies.Directory searchPatternStr changelogFileName               
            let (TagQuantity tagCount) = dependencies.Settings.TagsToLoad
            let! repo = dependencies.Git.tryGetRepo dependencies.Directory tagCount Version.versionPrefixes List.Empty dateTimeOffset                
            let! workspace, captureEvents = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! nextVersion =
                Version.tryStable workspace.Version dateTimeOffset
                |> Result.mapError CalafError.Domain
            let! releaseNotes, _ =
                tryReadCommits workspace dependencies.Git
                |> Result.map (fun commits -> tryChangeset commits nextVersion dateTimeOffset)
                |> Result.map (Option.map (fun (cs, events) ->
                    Some cs, Some events) >> Option.defaultValue (None, None))            
            let! workspace', releaseEvents =
                nextVersion
                |> Workspace.tryRelease workspace
                |> Result.mapError CalafError.Domain                
            let snapshot = Workspace.snapshot workspace' releaseNotes
            do! snapshot.Projects
                |> List.traverseResultM (fun p -> dependencies.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore            
            do! snapshot.Repository
                |> Option.map (fun p ->
                    dependencies.Git.tryApply (p.Directory, p.PendingFilesPaths) p.CommitText p.TagName
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
        | MakeType.Stable  -> tryStable2 dependencies
        | MakeType.Alpha   -> failwith "not implemented yet"
        | MakeType.Beta    -> failwith "not implemented yet"
        | MakeType.RC      -> failwith "not implemented yet"
        | MakeType.Nightly -> tryNightly2 dependencies      
        
    let run path arguments context settings  =
        let apply path arguments context settings =
            result {
                let! settings = settings
                let! cmd = input context.Console arguments
                match cmd with
                | Command.Make strategy ->
                    match strategy with
                    | MakeType.Nightly ->
                        return! tryMake path context settings Version.tryNightly
                    | MakeType.Alpha ->
                        return! tryMake path context settings Version.tryAlpha
                    | MakeType.Beta ->
                        return! tryMake path context settings Version.tryBeta
                    | MakeType.RC ->
                        return! tryMake path context settings Version.tryReleaseCandidate
                    | MakeType.Stable ->
                        return! tryMake path context settings Version.tryStable
            }
        let path = directory path
        let result = apply path arguments context settings
        result |> output context.Console |> exit