namespace Calaf.Infrastructure

open Argu

open Calaf.Application
open Calaf.Contracts

type internal MakeFlags =
    | [<CliPrefix(CliPrefix.DoubleDash)>] Changelog
    | [<CliPrefix(CliPrefix.DoubleDash); AltCommandLine("--include-prerelease")>] IncludePreRelease
    | [<CliPrefix(CliPrefix.DoubleDash)>] Projects of projects: string list
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Changelog ->
                "Generate a changelog. Default: off."
            | IncludePreRelease ->
                "Include pre-release changes in the changelog (ignores pre-release tags when computing the range). Requires `--changelog`. Default: off."
            | Projects _ ->
                "The directory or projects paths to scan for projects. Default is the current directory."
    static member ProjectsSearchDefaultPatterns = [ "*.?sproj" ]
                
type internal MakeCommand =    
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Stable of ParseResults<MakeFlags>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Alpha of ParseResults<MakeFlags>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Beta of ParseResults<MakeFlags>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] RC of ParseResults<MakeFlags>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Nightly of ParseResults<MakeFlags>
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
    let tryMakeFlags (changelogFlags: ParseResults<MakeFlags>) =
        let changelog = changelogFlags.Contains Changelog
        let includePreRelease = changelogFlags.Contains IncludePreRelease
        let projects =
            changelogFlags.TryGetResult Projects
            |> Option.defaultValue List.Empty
            |> List.filter (fun p -> not (System.String.IsNullOrWhiteSpace p))
            |> List.distinct
            |> fun ps ->
                if List.isEmpty ps
                then MakeFlags.ProjectsSearchDefaultPatterns
                else ps
        
        if includePreRelease && not changelog then
            ChangelogFlagRequired
            |> Input
            |> Error
        else
            Ok (changelog, includePreRelease, projects)
            
    let tryMakeCommand (commands: MakeCommand list) =
        let tryCmd ctor makeFlags =
            tryMakeFlags makeFlags
            |> Result.map (fun (changelog, includePreRelease, projects) -> 
                (ctor, changelog, includePreRelease, projects))
        match commands with
            | [ Nightly makeFlags ] -> tryCmd MakeType.Nightly makeFlags
            | [ Alpha makeFlags ]   -> tryCmd MakeType.Alpha makeFlags
            | [ Beta makeFlags ]    -> tryCmd MakeType.Beta makeFlags
            | [ RC makeFlags ]      -> tryCmd MakeType.RC makeFlags
            | [ Stable makeFlags ]  -> tryCmd MakeType.Stable makeFlags
            | [] ->               
                MakeCommandMissing
                |> Input
                |> Error
            | _  ->
                $"{commands.Head}"
                |> MakeCommandNotRecognized
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
            |> Result.map (fun (makeType, changelog, includePreRelease, projects) ->
                { Type = makeType
                  Changelog = changelog
                  IncludePreRelease = includePreRelease
                  Projects = projects }
                |> Command.Make)
        | [] ->
            { Type = MakeType.Stable
              Changelog = true
              IncludePreRelease = false
              Projects = MakeFlags.ProjectsSearchDefaultPatterns }
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