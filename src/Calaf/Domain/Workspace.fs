module internal Calaf.Domain.Workspace

open System.IO

open Calaf.Domain.DomainTypes

let create (directory: DirectoryInfo, projects: Project seq) : Workspace =    
    { Name = directory.Name
      Directory = directory.FullName
      Projects = projects
      Version = WorkspaceVersion.create projects }