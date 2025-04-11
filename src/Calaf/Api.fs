// Api. Composition Root
module rec Calaf.Api

open System.IO
open FsToolkit.ErrorHandling

open Calaf.Functions

let CreateWorkspace workingDir searchFilesPattern =
    let tryParseProject (metadata: ProjectMetadata) : Project option =
        option {
            let! xml = Xml.tryLoadXml(metadata.AbsolutePath)
            return! Project.tryCreate(metadata, xml)
        }        
    let loadProjects (workingDir : DirectoryInfo) =
        FileSystem.readFilesMatching searchFilesPattern workingDir
        |> Seq.map ProjectMetadata.create
        |> Seq.choose tryParseProject
        |> Seq.toArray
    
    let directory = WorkingDir.create workingDir
    let projects = directory |> loadProjects
    let version = WorkspaceVersion.create projects
    { Name = directory.Name
      Directory = directory.FullName
      Projects = projects
      Version = version }

let GetNextVersion (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
    option {
        let! propertyGroup = workspace.Version.PropertyGroup
        let! nextPropertyGroupVersion = Version.tryGetNext propertyGroup timeStamp |> Some
        return { PropertyGroup = nextPropertyGroupVersion }
    }