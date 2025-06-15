namespace Calaf.Application

open System
open FsToolkit.ErrorHandling
open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes.Entities

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
    
    let private release path context settings =
        result {
            let timeStamp = context.Clock.now()            
            let! monthStamp = timeStamp |> DateSteward.tryCreateMonthStamp |> Result.mapError CalafError.Domain                
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr                
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes timeStamp                
            let! workspace, _ = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain
            let! workspace', _ = Workspace.tryRelease workspace monthStamp |> Result.mapError CalafError.Domain                
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
    
    let private directory path =        
        if String.IsNullOrWhiteSpace path then "." else path    
        
    let private input (console: IConsole) (arguments: string[]) =
        Arguments.read console arguments
        
    let output (console: IConsole) result =
        match result with
        | Error error ->
            Console.error console error
            result
        | Ok workspace ->
            Console.ok console workspace
            result
            
    let exit result =
        match result with
        | Ok    _ -> 0
        | Error _ -> 1
        
    let run path arguments context settings  =
        let apply path arguments context settings =
            result {
                let! settings = settings
                let! cmd = input context.Console arguments
                match cmd with
                | Make strategy ->
                    match strategy with
                    | MakeType.Nightly ->
                        return! release path context settings
                    | MakeType.Release ->
                        return! release path context settings
            }
        let path = directory path
        let result = apply path arguments context settings
        result |> output context.Console |> exit