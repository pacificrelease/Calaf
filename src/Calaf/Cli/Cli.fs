namespace Calaf

open Argu

open Calaf.Contracts
open Calaf.CliError

type private MakeFlag =
    | [<CliPrefix(CliPrefix.DoubleDash)>]  Changelog
    | [<CliPrefix(CliPrefix.DoubleDash); AltCommandLine("--include-prerelease")>] IncludePreRelease
    | [<CliPrefix(CliPrefix.DoubleDash)>] Projects of projects: string list
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Changelog ->
                "Generate a changelog. By default the changelog will be generated for the stable release otherwise not."
            | IncludePreRelease ->
                "Include pre-release version's tags for generating changelog. Requires `--changelog`. Default is to skip."
             | Projects _ ->
                "The directory or projects paths to scan for projects. Default is the current directory."
    static member ProjectsSearchDefaultPatterns = [ "*.?sproj" ]
                
type private MakeCommand =    
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Stable  of ParseResults<MakeFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Nightly of ParseResults<MakeFlag>    
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Stable _ ->
                "Make a stable version"
            | Nightly _ ->
                "Make a nightly version"

type private InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make of ParseResults<MakeCommand>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make _ -> "Make a version."
    
module internal Cli =
    let private toDirectory path =        
        if System.String.IsNullOrWhiteSpace path
        then "."
        else path
        
    let private tryChangelogFlag (projectsFlags: ParseResults<MakeFlag>) =
        let changelog = projectsFlags.Contains Changelog
        let includePreRelease = projectsFlags.Contains IncludePreRelease
        let projects =
            projectsFlags.TryGetResult Projects
            |> Option.defaultValue List.Empty
            |> List.filter (fun p -> not (System.String.IsNullOrWhiteSpace p))
            |> List.distinct
            |> List.map toDirectory
            |> fun ps ->
                if List.isEmpty ps
                then MakeFlag.ProjectsSearchDefaultPatterns
                else ps
        
        if includePreRelease && not changelog then
            includePreReleaseChangelogRequired |> Error
        else
            Ok (changelog, includePreRelease, projects)
        
    let private tryMakeCommand (commands: MakeCommand list) =
        match commands with
        | [ Stable makeFlags ] -> 
            tryChangelogFlag makeFlags
            |> Result.map (fun (changelog, includePreRelease, makeFlags) -> 
                (MakeType.Stable, changelog, includePreRelease, makeFlags))
        | [ Nightly makeFlags ] -> 
            tryChangelogFlag makeFlags
            |> Result.map (fun (changelog, includePreRelease, makeFlags) -> 
                (MakeType.Nightly, changelog, includePreRelease,makeFlags))
        | [] -> 
            Ok (MakeType.Stable, false, false, MakeFlag.ProjectsSearchDefaultPatterns)
        | _ ->
            $"{commands.Head}" |> buildFlagNotRecognized |> Error   
                
    let private tryCommand (inputCommandResult: ParseResults<InputCommand>) =        
        let inputCommands = inputCommandResult.GetAllResults()
        match inputCommands with
        | [ Make makeCommandResults ] ->
            makeCommandResults.GetAllResults()
            |> tryMakeCommand
            |> Result.map (fun (makeType, changelog, includePreRelease, projects) ->
                { Type = makeType
                  Changelog = changelog
                  IncludePreRelease = includePreRelease
                  Projects = projects }
                |> Command.Make)            
        | [] ->
            { Type = MakeType.Stable
              Changelog = true
              IncludePreRelease = true
              Projects = MakeFlag.ProjectsSearchDefaultPatterns }
            |> Command.Make
            |> Ok
        | commands ->
            $"{commands.Head}"
            |> commandNotRecognized
            |> Error
                
    let private parse (args: string[]) =
        try            
            let parser = ArgumentParser.Create<InputCommand>()
            let inputCommandResult = parser.ParseCommandLine args
            inputCommandResult |> Ok
        with
        | exn ->
            argumentsFatal exn.Message |> Error            
    
    let tryCreateCommand (args: string[]) =
        args |> parse |> Result.bind tryCommand