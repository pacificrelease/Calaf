namespace Calaf.Contracts

type GitCommitInfo = {
    Hash: string
    Message: string
    When: System.DateTimeOffset
}

type GitTagInfo = {
    Name: string
    Commit: GitCommitInfo option
}

type GitRepositoryInfo = {
    Directory: string    
    Damaged: bool
    Unborn: bool
    Detached: bool    
    CurrentBranch: string option
    CurrentCommit: GitCommitInfo option
    Dirty: bool
    Tags: GitTagInfo[]
}

type ProjectInfo = {
    Name: string
    Directory: string
    Extension: string
    AbsolutePath: string
    Payload: System.Xml.Linq.XElement
}

type DirectoryInfo = {
    Directory: string
    Projects: ProjectInfo[]
}