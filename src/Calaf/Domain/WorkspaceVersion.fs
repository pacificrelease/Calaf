module internal Calaf.Domain.WorkspaceVersion

open Calaf.Domain.DomainTypes

let create (projects: Project seq) : WorkspaceVersion =
    let propertyGroup = projects
                            |> Project.chooseCalendarVersions
                            |> Version.tryMax 
    { PropertyGroup = propertyGroup }