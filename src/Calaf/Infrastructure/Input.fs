namespace Calaf.Infrastructure

open Argu

type internal MakeFlag =    
    | [<CliPrefix(CliPrefix.None)>] Release
    | [<CliPrefix(CliPrefix.None)>] Nightly
    interface IArgParserTemplate with
        member flag.Usage=
            match flag with
            | Release -> "Build a Release version"
            | Nightly -> "Build a Nightly version"

type internal InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make of ParseResults<MakeFlag>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make _ -> "Make a new workspace version."