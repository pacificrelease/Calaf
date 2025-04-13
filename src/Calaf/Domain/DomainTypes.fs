// Domain Types
namespace Calaf
   
type SuitableVersionPart = string
type Year  = uint16
type Month = uint8
type Patch = uint32

//YYYY.MM.PATCH
//YYYY.MM
type CalendarVersion = {
    Year:   Year
    Month:  Month
    Patch:  Patch option
}

type Version =
    | CalVer of CalendarVersion
    | LooksLikeSemVer of string
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
    | Eligible  of metadata: ProjectMetadata * lang: Language
    | Versioned of metadata: ProjectMetadata * lang: Language * currentVersion: Version
    | Bumped    of metadata: ProjectMetadata * lang: Language * previousVersion: CalendarVersion * currentVersion: CalendarVersion

type Workspace = {
    Name: string
    Directory : string
    Version : WorkspaceVersion
    Projects : Project[]
}