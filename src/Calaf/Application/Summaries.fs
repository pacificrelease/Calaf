namespace Calaf.Application

type ProjectSummary = {
    Name: string
    Directory: string
    Type: string
}

type GitSummary = {
    ReadyToCommit: bool
    Branch: string    
}

type WorkspaceSummary = {
    Directory: string    
    Version: string
    TrackedProjectsCount: uint32
    TotalProjectsCount: uint32
    Git: GitSummary option
}