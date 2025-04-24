module internal Calaf.Domain.WorkspaceVersion

open Calaf.Domain.DomainTypes

let create (projects: Project seq) : WorkspaceVersion =
    let propertyGroupVersion = projects
                            |> Project.chooseCalendarVersions
                            |> Version.tryMax 
    { PropertyGroup = propertyGroupVersion }