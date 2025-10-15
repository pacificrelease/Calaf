namespace Calaf

open Calaf.Application

type CliResponse = {
    ExitCode: int
    IsError: bool
    Text: string
}

module CliError =
    [<Literal>]
    let MisuseShellCommandOrInvalidArgumentsExitCode = 2
    
    let buildFlagNotRecognized (flag: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = $"Build flag '{flag}' is not recognized." }
        
    let commandNotRecognized (command: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = $"Command '{command}' is not recognized." }
        
    let includePreReleaseChangelogRequired: CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = "The '--include-prerelease' flag requires changelog generation via '--changelog' flag." }
        
    let argumentsFatal (message: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = message }
        
    let workspaceFatal (error: CalafError) : CliResponse =
        let message =
            match error with
            | Validation (BadWorkspacePath p) ->
                $"Workspace directory '{p}' has bad value"
            | _ ->                
                "Workspace directory path is wrong"            
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = message }
        
    let private mapInputError (inputError: InputError) =
        match inputError with
        | ChangelogFlagRequired ->
            { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
              IsError = true
              Text = "The '--changelog' flag is required." }
        | CommandNotRecognized c ->
            { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
              IsError = true
              Text = $"Command '{c}' is not recognized." }
        | _ ->
            { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
              IsError = true
              Text = "Bad input." }            
        
    let map (error: CalafError) : CliResponse =
        match error with
        | CalafError.Infrastructure (Input inputError) -> mapInputError inputError
        | _ ->
            { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
              IsError = true
              Text = "Application error." }