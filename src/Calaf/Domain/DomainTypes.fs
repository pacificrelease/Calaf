// Domain Types
namespace Calaf.Domain.DomainTypes

module Values =
    // Common
    type Patch = uint32
    // SemVer
    type Major = uint32
    type Minor = uint32
    // CalVer
    type Year  = uint16
    type Month = uint8

    // Value Object
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