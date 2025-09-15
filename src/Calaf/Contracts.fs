namespace Calaf.Contracts

// Incoming commands
type MakeType =    
    | Stable
    | Alpha
    | Beta
    | RC
    | Nightly
    
type Command =
    | Make of MakeType

// Contracts used by Infrastructure layer
type GitSignatureInfo = {
    Email: string
    Name: string
    When: System.DateTimeOffset
}

type GitCommitInfo = {
    Hash: string
    Text: string
    When: System.DateTimeOffset
}

type GitTagInfo = {
    Name: string
    Commit: GitCommitInfo option
}

type GitRepositoryInfo = {
    Directory: string
    Unborn: bool
    Detached: bool
    CurrentBranch: string option
    CurrentCommit: GitCommitInfo option
    Signature: GitSignatureInfo option
    Dirty: bool
    Tags: GitTagInfo list
}

type FileInfo = {
    Name: string
    Directory: string
    Extension: string
    AbsolutePath: string
    Exists: bool
}

type ProjectXmlFileInfo = {
    Info: FileInfo
    Content: System.Xml.Linq.XElement
}

type DirectoryInfo = {
    Directory: string
    Changelog: FileInfo
    Projects: ProjectXmlFileInfo list
}

// Summaries
type ReleaseType = | Stable | Nightly
type ReleaseSummary = {    
    Directory: string    
    VersionedProjectsCount: int
    TotalProjectsCount: int
    UsesGit: bool
    ReleaseType: ReleaseType
    PreviousRelease: string
    CurrentRelease: string
}

type CalafSummary =
    | Make of ReleaseSummary