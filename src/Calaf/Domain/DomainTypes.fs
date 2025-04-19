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

type Workspace = {
    Name: string
    Directory : string
    Version : WorkspaceVersion
    Projects : Project[]
}