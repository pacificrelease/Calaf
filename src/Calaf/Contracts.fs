namespace Calaf.Contracts

// Incoming commands
type MakeType =
    | Stable
    | Alpha
    | Beta
    | RC
    | Nightly
    
type MakeCommand = {
    Type: MakeType
    Changelog: bool
    IncludePreRelease: bool
    Projects: string list
}
    
type Command =
    | Make of MakeCommand

// Queries used by Infrastructure layer

//TODO: Move to Validation layer near the Program.fs
type ValidatedDirectoryInfo = ValidatedDirectoryInfo of string
type ValidatedFileInfo = ValidatedFileInfo of string
type internal ProjectsSearchScope =
    | FilesOnly of ValidatedFileInfo list
    | DirectoriesOnly of ValidatedDirectoryInfo list
    | FilesAndDirectories of directories: ValidatedDirectoryInfo list * files: ValidatedFileInfo list
    
//TODO Move to Application Contract
type internal ReadDirectoryQuery = {
    RootDirectory: ValidatedDirectoryInfo    
    ProjectsScope: ProjectsSearchScope option    
    SearchPatterns: string
    ChangelogFilename: string
}

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
    VersionTags: GitTagInfo list
    BaselineTags: GitTagInfo list option
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
type ReleaseType = | Stable | Alpha | Beta | RC | Nightly
type MadeSummary = {    
    Directory: string
    Git: bool
    Changelog: bool
    ReleaseType: ReleaseType
    PreviousVersion: string
    CurrentVersion: string
}

type CalafSummary =
    | Made of MadeSummary