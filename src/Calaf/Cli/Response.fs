namespace Calaf

// Response
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
        
    let argumentsFatal (message: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          IsError = true
          Text = message }