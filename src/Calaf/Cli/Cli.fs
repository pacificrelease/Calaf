namespace Calaf

open Argu

open Calaf.Contracts
open Calaf.CliError

type private MakeFlag =    
    | [<CliPrefix(CliPrefix.None)>] Stable
    | [<CliPrefix(CliPrefix.None)>] Nightly
    interface IArgParserTemplate with
        member flag.Usage =
            match flag with
            | Stable  -> "Make a stable version"
            | Nightly -> "Make a nightly version"

type private InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make of ParseResults<MakeFlag>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make _ -> "Make a workspace version."
    
module internal Cli =
    let private toDirectory path =        
        if System.String.IsNullOrWhiteSpace path
        then "."
        else path
        
    let private tryMakeFlag (flags: MakeFlag list) =
        match flags with
        | [ Nightly ] -> Ok MakeType.Nightly
        | [ Stable ]  -> Ok MakeType.Stable
        | [] -> Ok MakeType.Stable
        | _  ->
            $"{flags.Head}" |> buildFlagNotRecognized |> Error   
                
    let private tryCommand (inputCommandResult: ParseResults<InputCommand>) =
        let inputCommands = inputCommandResult.GetAllResults()
        match inputCommands with
        | [ Make makeFlagsResults ] ->
            let makeFlags = makeFlagsResults.GetAllResults()
            makeFlags |> tryMakeFlag |> Result.map Command.Make
        | [] -> MakeType.Stable |> Command.Make |> Ok
        | commands ->
            $"{commands.Head}" |> commandNotRecognized |> Error
                
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