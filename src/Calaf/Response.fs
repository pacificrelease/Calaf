namespace Calaf

// Summaries
type ReleaseSummary = {    
    Directory: string
    VersionedProjectsCount: int
    TotalProjectsCount: int
    UsesGit: bool
    PreviousRelease: string
    CurrentRelease: string
}

// Response
type internal CliResponse = {
    ExitCode: int
    Text: string
}

module internal CliErrorResponses =
    [<Literal>]
    let MisuseShellCommandOrInvalidArgumentsExitCode = 2
    
    let buildFlagNotRecognized (flag: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          Text = $"Build flag '{flag}' is not recognized." }
        
    let commandNotRecognized (command: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          Text = $"Command '{command}' is not recognized." }
        
    let argumentsFatal (message: string) : CliResponse =
        { ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode
          Text = message }