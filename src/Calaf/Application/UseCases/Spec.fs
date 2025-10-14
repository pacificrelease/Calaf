namespace Calaf.Application

type VersionType = | Stable | Alpha | Beta | ReleaseCandidate | Nightly

type ChangelogGeneration = {
    IncludePreRelease: bool
}
type MakeSpec = {
    WorkspaceDirectory: string
    VersionType: VersionType
    ChangelogGeneration: ChangelogGeneration option
    TargetProjects: string list option
}