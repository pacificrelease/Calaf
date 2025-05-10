// Domain Types
namespace Calaf.Domain.DomainTypes

// Common
type Patch = uint32
// SemVer
type Major = uint32
type Minor = uint32
// CalVer
type Year  = uint16
type Month = uint8

type MonthStamp = {
    Year:   Year
    Month:  Month
}

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

// Repository
type CommitMessage = string
type CommitHash = string
type TagName = string
type BranchName = string

type Commit = {
    Message: CommitMessage
    Hash: CommitHash
    When: System.DateTimeOffset
}

type Tag =
    | Unversioned of name: TagName
    | Versioned   of name: TagName * version: Version * commit: Commit option

type Head =
    | Attached of commit: Commit * branchName: BranchName
    | Detached of commit: Commit    

type Repository =
    | Damaged of directory: string
    | Unborn  of directory: string 
    | Dirty   of directory: string * head: Head * currentVersion: CalendarVersion option
    | Ready   of directory: string * head: Head * currentVersion: CalendarVersion option
    | Bumped  of directory: string * previousVersion: CalendarVersion * currentVersion: CalendarVersion

// Project
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

type Suite = {
    Version: CalendarVersion option
    Projects: Project[]    
}

type Workspace = {
    Directory: string
    Repository: Repository option
    Suite: Suite
}
    