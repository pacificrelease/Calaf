namespace Calaf.Shell

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

