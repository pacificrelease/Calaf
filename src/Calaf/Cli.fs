namespace Calaf

open Argu

open Calaf.Contracts

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
            
type CliError =
    | BuildFlagNotRecognized of string
    | CommandNotRecognized of string
    
module internal Cli =
    let private createBuildFlagNotRecognized (flag: string) : Response =
        { ExitCode = 1
          Text = $"Build flag '{flag}' is not recognized." }
        
    let private createCommandNotRecognized (command: string) : Response =
        { ExitCode = 1
          Text = $"Command '{command}' is not recognized." }
    
    let private reduceMakeFlags (flags: MakeFlag list) =
        match flags with
        | [ Nightly ] -> Ok MakeType.Nightly
        | [ Stable ]  -> Ok MakeType.Stable
        | [] -> Ok MakeType.Stable
        | _  ->
            $"{flags.Head}" |> createBuildFlagNotRecognized |> Error   
                
    let private toCommand (results: ParseResults<InputCommand>) =
        match results.GetAllResults() with
        | [ Make makeFlagsResults ] ->
            let makeFlags = makeFlagsResults.GetAllResults()
            makeFlags |> reduceMakeFlags |> Result.map Command.Make
        | [] -> MakeType.Stable |> Command.Make |> Ok
        | commands ->
            $"{commands.Head}" |> createCommandNotRecognized |> Error
                
    let private parse (args: string[]) =
        let parser = ArgumentParser.Create<InputCommand>()
        parser.ParseCommandLine args
    
    let command (args: string[]) =
        args |> parse |> toCommand