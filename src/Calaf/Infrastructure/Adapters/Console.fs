namespace Calaf.Infrastructure

open Argu

open Calaf.Application
open Calaf.Contracts

type MakeFlag =    
    | [<CliPrefix(CliPrefix.None)>] Release
    | [<CliPrefix(CliPrefix.None)>] Nightly
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Release -> "Make a Release version"
            | Nightly -> "Make a Nightly version"

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
            | [ Release ] -> Ok MakeType.Release
            | [] -> Ok MakeType.Release
            | _  ->
                $"{flags.Head}"
                |> BuildFlagNotRecognized
                |> Input
                |> Error    
                
module internal ConsoleInput =      
    let parse (args: string[]) =
        let parser = ArgumentParser.Create<InputCommand>()
        parser.ParseCommandLine(args)

    let read (results: ParseResults<InputCommand>) =
        match results.GetAllResults() with
        | [ Make makeFlagsResults ] ->
            makeFlagsResults.GetAllResults()
            |> ConsoleInputGateway.toMakeType |> Result.map Command.Make
        | [] -> MakeType.Release |> Command.Make |> Ok
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