// Domain Types
namespace Calaf.Domain.DomainTypes

// Common
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

// Value Object
// MAJOR.MINOR.PATCH-SUFFIX
type SemanticVersion = {
    Major: Major
    Minor: Minor
    Patch: Patch
}

//YYYY.MM.MICRO-SUFFIX
//YYYY.MM
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
// Repository
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
    BreakingChange: bool
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
    Version: RepositoryVersion option
}
// DU for events
[<Struct>] 
type RepositoryState = | Damaged | Unsigned | Unborn | Dirty | Ready

type VersionedTag = {
    Name: TagName
    Version: Version
    Commit: Commit option
}    

type Changeset = {
    Features : ConventionalCommitMessage list
    Fixes : ConventionalCommitMessage list
    BreakingChanges : ConventionalCommitMessage list
    Other: CommitText list
}    

// File System
[<Struct>]
type Language =
    | FSharp
    | CSharp
    
type ProjectMetadata = {
    Name : string
    Extension: string
    Directory : string
    AbsolutePath : string
}

type ProjectContent = 
    | Xml  of System.Xml.Linq.XElement
    | Json of System.Text.Json.JsonDocument
    
type UnversionedProject = {
    Metadata: ProjectMetadata
    Language: Language
}

type VersionedProject = {
    Metadata: ProjectMetadata
    Language: Language
    Content: ProjectContent
    Version: Version
}

type ProjectActionSnapshot = {
    AbsolutePath: string
    Content: System.Xml.Linq.XElement
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
    Repository: RepositoryActionSnapshot option
}

type Tag =
    | Versioned   of VersionedTag
    | Unversioned of name: TagName

type Repository =
    | Damaged  of directory: string
    | Unsigned of directory: string
    | Unborn   of directory: string
    | Dirty    of directory: string * metadata: RepositoryMetadata
    | Ready    of directory: string * metadata: RepositoryMetadata

type Project =
    | Versioned   of VersionedProject
    | Unversioned of UnversionedProject    

type Collection =
    | Standard of version: CalendarVersion * projects: Project list

type Workspace = {
    Directory: string
    Version: CalendarVersion
    Repository: Repository option
    Collection: Collection
}