// Domain Types
namespace Calaf.Domain.DomainTypes

type Major = uint32
type Minor = uint32

type Year  = uint16
type Month = uint8
type Patch = uint32

// MAJOR.MINOR.PATCH
type SemanticVersion = {
    Major: Major
    Minor: Minor
    Patch: Patch
}

//YYYY.MM.PATCH
//YYYY.MM
type CalendarVersion = {
    Year:   Year
    Month:  Month
    Patch:  Patch option
}

type Version =
    | CalVer of CalendarVersion
    | SemVer of SemanticVersion
    | Unsupported
    
type WorkspaceVersion = {
    PropertyGroup: CalendarVersion option
}

type Language =
    | FSharp
    | CSharp

type ProjectMetadata = {
    Name : string
    Extension: string
    Directory : string
    AbsolutePath : string
}

type Project =
    | Unversioned of metadata: ProjectMetadata * lang: Language
    | Versioned   of metadata: ProjectMetadata * lang: Language * currentVersion: Version
    | Bumped      of metadata: ProjectMetadata * lang: Language * previousVersion: CalendarVersion * currentVersion: CalendarVersion
    | Skipped     of metadata: ProjectMetadata * lang: Language * currentVersion: Version

type CommitHash = string

type GitCommit = {
    Hash: CommitHash
    Message: string
}

type GitTag = {
    Name: string
    Version: CalendarVersion
    Commit: GitCommit
}

type GitHead =
    | Detached of commit: GitCommit
    | Branch   of commit: GitCommit * branch: string

type GitRepositoryDetails = {
    Directory: string
    Head: GitHead
    Tags: GitTag[]
}

type GitRepositoryState =
    | Dirty of details: GitRepositoryDetails
    | Ready of details: GitRepositoryDetails * currentTag: GitTag

type Workspace = {
    Name: string
    Directory : string
    Version : WorkspaceVersion
    Projects : Project seq
}