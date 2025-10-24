namespace Calaf

open Argu

type private ValidatedDirectoryFullPath = ValidatedDirectoryFullPath of string
type private ValidatedProjectPath = private {
    ValidatedFullPath: string
    ValidatedRelativePath: string
}

type private MakeFlag2 =
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
                
type private MakeCommand2 =    
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Stable of ParseResults<MakeFlag2>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Alpha of ParseResults<MakeFlag2>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Beta of ParseResults<MakeFlag2>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] RC of ParseResults<MakeFlag2>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Nightly of ParseResults<MakeFlag2>
    interface IArgParserTemplate with
        member cmd.Usage =
            match cmd with
            | Stable _ -> "Create a stable release"
            | Alpha _ -> "Create an alpha release"
            | Beta _ -> "Create a beta release"
            | RC _ -> "Create a release candidate"
            | Nightly _ -> "Create a nightly build"

type private InputCommand2 = 
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Make2 of ParseResults<MakeCommand2>
    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Make2 _ -> "Create a new workspace release"