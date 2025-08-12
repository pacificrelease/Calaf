// Domain Types
namespace Calaf.Domain.DomainTypes

module Values =
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

    type Version =
        | CalVer of CalendarVersion
        | SemVer of SemanticVersion
        | Unsupported

    // Repository
    type SignatureName = string
    type SignatureEmail = string
    type CommitMessage = string
    type CommitHash = string
    type TagName = string
    type BranchName = string

    type Signature = {
        Name: SignatureName
        Email: SignatureEmail
        When: System.DateTimeOffset
    }

    type Commit = {
        Message: CommitMessage
        Hash: CommitHash
        When: System.DateTimeOffset
    }
    
    type Head =
        | Attached of commit: Commit * branchName: BranchName
        | Detached of commit: Commit

    // DU for events
    [<Struct>] 
    type RepositoryState = | Damaged | Unsigned | Unborn | Dirty | Ready
    
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
    
    type ProjectActionProfile = {
        AbsolutePath: string
        Content: System.Xml.Linq.XElement
    }
    
    type RepositoryActionProfile = {
        Directory: string
        Files: string list
        Signature: Signature
        TagName: TagName
        CommitMessage: CommitMessage
    }   
    
    type WorkspaceActionProfile = {        
        Projects: ProjectActionProfile list
        Repository: RepositoryActionProfile option
    }

module Entities =
    open Values
    
    type Tag =
        | Versioned   of name: TagName * version: Version * commit: Commit option
        | Unversioned of name: TagName

    type Repository =
        | Damaged  of directory: string
        | Unsigned of directory: string
        | Unborn   of directory: string
        | Dirty    of directory: string * head: Head * signature: Signature * version: CalendarVersion option
        | Ready    of directory: string * head: Head * signature: Signature * version: CalendarVersion option

    type Project =
        | Versioned   of VersionedProject
        | Unversioned of UnversionedProject    

    type Suite =
        | StandardSet of version: CalendarVersion * projects: Project list

    type Workspace = {
        Directory: string
        Version: CalendarVersion
        Repository: Repository option
        Suite: Suite
    }