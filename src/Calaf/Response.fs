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