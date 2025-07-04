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
type internal Response = {
    ExitCode: int
    Text: string
}

module internal CliErrorResponses =
    let buildFlagNotRecognized (flag: string) : Response =
        { ExitCode = 1
          Text = $"Build flag '{flag}' is not recognized." }
        
    let commandNotRecognized (command: string) : Response =
        { ExitCode = 1
          Text = $"Command '{command}' is not recognized." }
        
    let argumentsFatal (message: string) : Response =
        { ExitCode = 1
          Text = message }