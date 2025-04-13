namespace Calaf

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal WorkspaceVersion =    
    let create (projects: Project[]) : WorkspaceVersion =
        let propertyGroupVersion = projects
                                |> Project.chooseCalendarVersions
                                |> Version.tryMax 
        { PropertyGroup = propertyGroupVersion }