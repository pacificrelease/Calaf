module internal Calaf.Domain.WorkspaceVersion

open Calaf.Domain.DomainTypes

let create (projects: Project[]) : WorkspaceVersion =
    let propertyGroupVersion = projects
                            |> Project.chooseCalendarVersions
                            |> Version.tryMax 
    { PropertyGroup = propertyGroupVersion }
    
    
// let bump (workspaceVersion: WorkspaceVersion, timeStamp: System.DateTime) : WorkspaceVersion =
//     let result = Version.tryBump workspaceVersion.PropertyGroup timeStamp
//                 |> Option.bind(workspaceVersion. nextVersion)
//                 |> Option.map()
    