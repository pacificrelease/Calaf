namespace Calaf.Infrastructure

open Calaf.Application
open Calaf.Contracts

module internal Arguments =
    open Argu

    type BuildFlag =    
        | [<CliPrefix(CliPrefix.None)>] Release
        | [<CliPrefix(CliPrefix.None)>] Nightly
        | [<CliPrefix(CliPrefix.None)>] Preview
        interface IArgParserTemplate with
            member flag.Usage =
                match flag with
                | Release -> "Build a Release version"
                | Nightly -> "Build a Nightly version"
                | Preview -> "Build a Preview version"

    type CommandArg = 
        | [<SubCommand; CliPrefix(CliPrefix.None)>] Build of ParseResults<BuildFlag>
        interface IArgParserTemplate with
            member command.Usage =
                match command with
                | Build _ -> "Build a workspace version."
                
module internal ConsoleInputGateway =    
    open Calaf.Infrastructure
    
    let toBuildType (flags: BuildFlag list) =
        match flags with
            | [ Nightly ] -> Ok BuildType.Nightly
            | [ Release ] -> Ok BuildType.Release
            | [] -> Ok BuildType.Release
            | _  ->
                $"{flags.Head}"
                |> BuildFlagNotRecognized
                |> Input
                |> Error    
                
module internal ConsoleInput =    
    open Argu
    
    open Calaf.Infrastructure
    
    let parse (args: string[]) =
        let parser = ArgumentParser.Create<InputCommand>()
        parser.ParseCommandLine(args)

    let read (results: ParseResults<InputCommand>) =
        match results.GetAllResults() with
        | [ Build buildFlagsResults ] ->
            buildFlagsResults.GetAllResults()
            |> ConsoleInputGateway.toBuildType |> Result.map Command.Build
        | [] -> BuildType.Release |> Command.Build |> Ok
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