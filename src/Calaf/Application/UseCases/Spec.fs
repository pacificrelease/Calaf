namespace Calaf.Application

type ReleaseType = | Stable | Alpha | Beta | RC | Nightly
type Changelog =
    | Disabled
    | Enabled of includePreRelease: bool
type AllProjectsSpec = {
    SearchPatterns: string list
}
type TargetProjectSpec = {
    AbsolutePath: string
    RelativePath: string
}
type ProjectsScope =
    | AllProjects of AllProjectsSpec
    | TargetProjects of TargetProjectSpec list
type MakeSpec = {
    WorkspaceDirectory: string
    ReleaseType: ReleaseType
    Changelog: Changelog
    ProjectsScope: ProjectsScope
}