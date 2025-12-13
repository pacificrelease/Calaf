namespace Calaf.Core.Domain.DomainTypes

// #Common Types
type TimeStamp = System.DateTimeOffset
type Patch = uint32
type Micro = uint32
type DayOfMonth = uint8
type BuildNumber = uint16
type NightlyBuild = {
    Day:    DayOfMonth
    Number: BuildNumber
}
type AlphaBuild = {
    Number: BuildNumber
}
type BetaBuild = {
    Number: BuildNumber
}
type ReleaseCandidateBuild = {
    Number: BuildNumber
}
type Build =
    | Nightly                 of NightlyBuild
    | Alpha                   of AlphaBuild
    | AlphaNightly            of alpha: AlphaBuild * nightly: NightlyBuild
    | Beta                    of BetaBuild
    | BetaNightly             of beta: BetaBuild * nightly: NightlyBuild
    | ReleaseCandidate        of ReleaseCandidateBuild
    | ReleaseCandidateNightly of releaseCandidate: ReleaseCandidateBuild * nightly: NightlyBuild
// SemVer
type Major = uint32
type Minor = uint32
// CalVer
type Year  = uint16
type Month = uint8

// MAJOR.MINOR.PATCH[-SUFFIX]
type SemanticVersion = {
    Major: Major
    Minor: Minor
    Patch: Patch
}

//YYYY.MM[.MICRO-SUFFIX]
type CalendarVersion = {
    Year:  Year
    Month: Month
    Micro: Micro option
    Build: Build option
}
type VersionSource =
    | Tag     of tagName: string
    | Project of absolutePath : string
type Version =
    | CalVer of CalendarVersion
    | SemVer of SemanticVersion
    | Unsupported
    
// Git Repository Types
type BranchName = string
type SignatureName = string
type SignatureEmail = string
type TagName = string
type CommitHash = string
type CommitText = string
type CommitScope = string option
type CommitDescription = string
type BreakingChange = bool
type ConventionalCommitMessage = {
    _type: string
    _scope: string
    _breakingChange: string    
    _splitter: string
    Scope: CommitScope
    Description: CommitDescription
    BreakingChange: BreakingChange
}
type CommitMessage =
    | Feature of conventionalMessage: ConventionalCommitMessage
    | Fix     of conventionalMessage: ConventionalCommitMessage
    | Other   of message: CommitText
    | Empty
type Signature = {
    Name: SignatureName
    Email: SignatureEmail
    When: System.DateTimeOffset
}
type Commit = {
    Message: CommitMessage
    Text: CommitText        
    Hash: CommitHash
    When: System.DateTimeOffset
}
type Head =
    | Attached of commit: Commit * branchName: BranchName
    | Detached of commit: Commit
type RepositoryVersion = {
    TagName: TagName
    CommitMessage: CommitMessage option
    Version: Version
}
type RepositoryMetadata = {
    Head: Head
    Signature: Signature
    CurrentVersion: RepositoryVersion option
    BaselineVersion: RepositoryVersion option
}
// DU for events
[<Struct>] 
type RepositoryState =
    | Damaged
    | Unsigned
    | Unborn
    | Dirty
    | Ready
type VersionedTag = {
    Name: TagName
    Version: Version
    Commit: Commit option
}
type ReleaseChangeSet = {
    Features : ConventionalCommitMessage list
    Fixes : ConventionalCommitMessage list
    BreakingChanges : ConventionalCommitMessage list
    Other: CommitText list
    Version: CalendarVersion
    TimeStamp: TimeStamp
}
type ReleaseNotes =
    | WithChanges    of ReleaseChangeSet
    | WithoutChanges of version: CalendarVersion * timeStamp: TimeStamp
    
type Tag =
    | Versioned   of VersionedTag
    | Unversioned of name: TagName
type Repository =
    | Damaged  of directory: string
    | Unsigned of directory: string
    | Unborn   of directory: string
    | Dirty    of directory: string * metadata: RepositoryMetadata
    | Ready    of directory: string * metadata: RepositoryMetadata

// File System Types
type FileMetadata = {
    FileName : string
    FileExtension: string
    Directory : string
    AbsolutePath : string
}
[<Struct>]
type Language =
    | FSharp
    | CSharp    
type ProjectContent = 
    | Xml of content: System.Xml.Linq.XElement    
type UnversionedProject = {
    Metadata: FileMetadata
    Language: Language
}
type VersionedProject = {
    Metadata: FileMetadata
    Language: Language
    Content: ProjectContent
    CurrentVersion: Version
}
type Project =
    | Versioned   of VersionedProject
    | Unversioned of UnversionedProject
type Changelog = {
    Metadata: FileMetadata
    ExistInFileSystem: bool
}
type StandardBundle = {
    CurrentVersion: CalendarVersion
    Projects: VersionedProject list
    Changelog: Changelog
}
// Workbench – a collection of projects being worked on
// Portfolio – a set of related projects
// Bundle – a grouped set of projects
// Collection – a generic term for grouped items
// Suite – a set of related components
// Assembly – a gathered set of projects
// Codebase – the collection of source code/projects
// ProjectSet – explicit and descriptive
// ProjectGroup – emphasizes grouping
// Manifest – a declared set of tracked items
type Bundle =
    | Standard of StandardBundle

// Workspace Types
type Workspace = {
    CurrentDirectory: string    
    CurrentVersion: CalendarVersion
    Repository: Repository option
    Projects: Bundle
}




// END_OF_SNAPSHOTS
type ProjectActionSnapshot = {
    AbsolutePath: string
    Content: System.Xml.Linq.XElement
}

type ChangelogActionSnapshot = {
    AbsolutePath: string
    ExistInFileSystem: bool
    ReleaseNotesContent: string
}
type RepositoryActionSnapshot = {
    Directory: string
    PendingFilesPaths: string list
    Signature: Signature
    TagName: TagName
    CommitText: CommitText
}
type WorkspaceActionSnapshot = {        
    Projects: ProjectActionSnapshot list
    Changelog: ChangelogActionSnapshot option
    Repository: RepositoryActionSnapshot option
}
// END_OF_SNAPSHOTS