namespace Calaf.Application

type VersionType = | Stable | Alpha | Beta | ReleaseCandidate | Nightly

type ChangelogGeneration = {
    IncludePreRelease: bool
}
type TargetProject = {
    FullPath: string
    RelativePath: string
}
type MakeSpec = {
    WorkspaceDirectory: string
    VersionType: VersionType
    ChangelogGeneration: ChangelogGeneration option
    TargetProjects: TargetProject list option
}