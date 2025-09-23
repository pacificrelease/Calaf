namespace Calaf.Infrastructure

open Argu

open Calaf.Application
open Calaf.Contracts

type MakeFlag =    
    | [<CliPrefix(CliPrefix.None)>] Stable
    | [<CliPrefix(CliPrefix.None)>] Alpha
    | [<CliPrefix(CliPrefix.None)>] Beta
    | [<CliPrefix(CliPrefix.None)>] RC
    | [<CliPrefix(CliPrefix.None)>] Nightly
    | [<CliPrefix(CliPrefix.DoubleDash);
        AltCommandLine("--no-changelog")>] NoChangelog of bool
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Stable  -> "Make a stable version"
            | Alpha   -> "Make an alpha version"
            | Beta    -> "Make a beta version"
            | RC      -> "Make a release candidate version"
            | Nightly -> "Make a nightly version"
            | NoChangelog _ -> "Do not update changelog: --no-changelog [true|false] (default: false)"

type InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make of ParseResults<MakeFlag>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make _ -> "Make a workspace version."
                
module internal ConsoleInputGateway =     
    let toMakeType (flags: MakeFlag list) =
        match flags with
            | [ Nightly ] -> Ok MakeType.Nightly
            | [ Alpha ]   -> Ok MakeType.Alpha
            | [ Beta ]    -> Ok MakeType.Beta
            | [ RC ]      -> Ok MakeType.RC
            | [ Stable ]  -> Ok MakeType.Stable            
            | [] -> Ok MakeType.Stable
            | _  ->
                $"{flags.Head}"
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
        | [ Make makeFlagsResults ] ->
            makeFlagsResults.GetAllResults()
            |> List.choose (function
                | Nightly -> Some Nightly
                | Alpha   -> Some Alpha
                | Beta    -> Some Beta
                | RC      -> Some RC
                | Stable  -> Some Stable
                | _       -> None)
            |> ConsoleInputGateway.toMakeType
            |> Result.map (fun makeType ->
                let changelog =
                    makeFlagsResults.TryGetResult NoChangelog
                    |> Option.defaultValue false
                { Type = makeType
                  ChangeLog = changelog }
                |> Command.Make)         
        | [] ->
            { Type = MakeType.Stable; ChangeLog = false }
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