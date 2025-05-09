namespace Calaf.Application

type SkippedProjectSummary = {
    Name: string
    Directory: string
    Type: string
    SkipReason: string
}

type TrackedProjectSummary = {
    Name: string
    Directory: string
    Type: string
    Version: string    
}

type GitSummary = {
    ReadyToCommit: bool
    Branch: string
    LastTag: string option
}

type WorkspaceSummary = {
    Directory: string    
    Version: string
    TrackedProjectsCount: uint32
    SkippedProjectsCount: uint32
    TotalProjectsCount: uint32
    Git: GitSummary option
}