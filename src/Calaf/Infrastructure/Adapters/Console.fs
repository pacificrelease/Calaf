namespace Calaf.Infrastructure

open Argu

open Calaf.Application
open Calaf.Contracts

type internal ChangelogFlag =
    | [<CliPrefix(CliPrefix.DoubleDash)>] Changelog
    | [<CliPrefix(CliPrefix.DoubleDash); AltCommandLine("--include-prerelease")>] IncludePreRelease
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Changelog ->
                "Generate a changelog. Default: off."
            | IncludePreRelease ->
                "Include pre-release changes in the changelog (ignores pre-release tags when computing the range). Requires `--changelog`. Default: off."
                
type internal MakeCommand =    
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Stable of ParseResults<ChangelogFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Alpha of ParseResults<ChangelogFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Beta of ParseResults<ChangelogFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] RC of ParseResults<ChangelogFlag>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Nightly of ParseResults<ChangelogFlag>
    interface IArgParserTemplate with
        member cmd.Usage =
            match cmd with
            | Stable _  -> "Create a stable release"
            | Alpha _   -> "Create an alpha release"
            | Beta _    -> "Create a beta release"
            | RC _      -> "Create a release candidate"
            | Nightly _ -> "Create a nightly build"

type internal InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make of ParseResults<MakeCommand>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make _ -> "Create a workspace release"
                
module internal ConsoleInputGateway =
    let tryChangelogFlag (changelogFlags: ParseResults<ChangelogFlag>) =
        let changelog = changelogFlags.Contains Changelog
        let includePreRelease = changelogFlags.Contains IncludePreRelease
        
        if includePreRelease && not changelog then
            ChangelogRequiredToIncludePrerelease
            |> Input
            |> Error
        else
            Ok (changelog, includePreRelease)
            
    let tryMakeCommand (commands: MakeCommand list) =
        let tryCmd ctor flags =
            tryChangelogFlag flags
            |> Result.map (fun (changelog, includePreRelease) -> 
                (ctor, changelog, includePreRelease))
        match commands with
            | [ Nightly changelogFlags ] -> tryCmd MakeType.Nightly changelogFlags
            | [ Alpha changelogFlags ]   -> tryCmd MakeType.Alpha changelogFlags
            | [ Beta changelogFlags ]    -> tryCmd MakeType.Beta changelogFlags
            | [ RC changelogFlags ]      -> tryCmd MakeType.RC changelogFlags
            | [ Stable changelogFlags ]  -> tryCmd MakeType.Stable changelogFlags
            | [] -> Ok (MakeType.Stable, false, false)
            | _  ->
                $"{commands.Head}"
                |> MakeFlagNotRecognized
                |> Input
                |> Error    
                
module internal ConsoleInput =      
    let parse (args: string[]) =
        let parser = ArgumentParser.Create<InputCommand>()
        parser.ParseCommandLine(args)

    let read (inputCommandResult: ParseResults<InputCommand>) =
        let inputCommands = inputCommandResult.GetAllResults()
        match inputCommands with
        | [ Make makeCommandResults ] ->
            makeCommandResults.GetAllResults()
            |> ConsoleInputGateway.tryMakeCommand
            |> Result.map (fun (makeType, changelog, includePreRelease) ->
                { Type = makeType
                  Changelog = changelog
                  IncludePreRelease = includePreRelease }
                |> Command.Make)
        | [] ->
            { Type = MakeType.Stable
              Changelog = true
              IncludePreRelease = false }
            |> Command.Make
            |> Ok
        | commands ->
            $"{commands.Head}"
            |> CommandNotRecognized
            |> Input
            |> Error                    

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ConsoleOutput =
    let private setForegroundColor color =
        System.Console.ForegroundColor <- color
    
    let private resetForegroundColor () =
        System.Console.ResetColor()
        
    let writeLine (message: string) =
        System.Console.WriteLine(message)
        
    let writeError (message: string) =
        setForegroundColor System.ConsoleColor.Red
        writeLine message
        resetForegroundColor ()
        
    let writeSuccess (message: string) =
        setForegroundColor System.ConsoleColor.Green
        writeLine message
        resetForegroundColor ()    

type Console() =
    interface IConsole with
        member _.read (args: string[]) =
            args
            |> ConsoleInput.parse
            |> ConsoleInput.read
            |> Result.mapError CalafError.Infrastructure
            
        member _.write (message: string) =            
            message |> ConsoleOutput.writeLine           
            
        member _.success (message: string) =            
            message |> ConsoleOutput.writeSuccess

        member _.error (message: string) =
            message |> ConsoleOutput.writeError