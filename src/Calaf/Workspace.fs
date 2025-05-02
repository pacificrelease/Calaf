module internal Calaf.Domain.Workspace

open System.IO

open Calaf.Domain.DomainTypes

let create (directory: DirectoryInfo, projects: Project seq) : Workspace =
    let workspaceVersion = WorkspaceVersion.create projects
    { Name = directory.Name
      Directory = directory.FullName
      Projects = projects
      Version = workspaceVersion }