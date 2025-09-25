namespace Calaf

open Argu

open Calaf.Contracts
open Calaf.CliError

type private ChangelogFlag =
    | [<CliPrefix(CliPrefix.DoubleDash)>]  Changelog
    | [<CliPrefix(CliPrefix.DoubleDash); AltCommandLine("--include-prerelease")>] IncludePreRelease
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Changelog ->
                "Generate a changelog. By default the changelog will be generated for the stable release otherwise not."
            | IncludePreRelease ->
                "Include pre-release version's tags for generating changelog. Requires `--changelog`. Default is to skip."
                
type private MakeCommand =    
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Stable  of ParseResults<ChangelogFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Nightly of ParseResults<ChangelogFlag>    
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
        
    let private tryChangelogFlag (changelogFlags: ParseResults<ChangelogFlag>) =
        let changelog = changelogFlags.Contains Changelog
        let includePreRelease = changelogFlags.Contains IncludePreRelease
        
        if includePreRelease && not changelog then
            includePreReleaseChangelogRequired |> Error
        else
            Ok (changelog, includePreRelease)
        
    let private tryMakeCommand (commands: MakeCommand list) =
        match commands with
        | [ Stable changelogFlags ] -> 
            tryChangelogFlag changelogFlags
            |> Result.map (fun (changelog, includePreRelease) -> 
                (MakeType.Stable, changelog, includePreRelease))
        | [ Nightly changelogFlags ] -> 
            tryChangelogFlag changelogFlags
            |> Result.map (fun (changelog, includePreRelease) -> 
                (MakeType.Nightly, changelog, includePreRelease))
        | [] -> 
            Ok (MakeType.Stable, false, false)
        | _ ->
            $"{commands.Head}" |> buildFlagNotRecognized |> Error   
                
    let private tryCommand (inputCommandResult: ParseResults<InputCommand>) =        
        let inputCommands = inputCommandResult.GetAllResults()
        match inputCommands with
        | [ Make makeCommandResults ] ->
            makeCommandResults.GetAllResults()
            |> tryMakeCommand
            |> Result.map (fun (makeType, changelog, includePreRelease) ->
                { Type = makeType
                  Changelog = changelog
                  IncludePreRelease = includePreRelease }
                |> Command.Make)            
        | [] ->
            { Type = MakeType.Stable; Changelog = true; IncludePreRelease = true }
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