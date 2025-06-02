namespace Calaf.Infrastructure

open Argu

type internal BuildFlag =    
    | [<CliPrefix(CliPrefix.None)>] Release
    | [<CliPrefix(CliPrefix.None)>] Nightly
    interface IArgParserTemplate with
        member flag.Usage=
            match flag with
            | Release -> "Build a Release version"
            | Nightly -> "Build a Nightly version"

type internal InputCommand = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Build of ParseResults<BuildFlag>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Build _ -> "Build a workspace version."