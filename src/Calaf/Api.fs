// Api. Composition Root
module rec Calaf.Api

open System.IO
open FsToolkit.ErrorHandling

open Calaf.Errors
open Calaf.Functions

let CreateWorkspace workingDir searchFilesPattern : Result<Workspace, DomainError> =
    result {             
        let tryParseProject (metadata: ProjectMetadata) : Project option =
            option {
                let! xml = Xml.TryLoadXml(metadata.AbsolutePath)
                return! Project.tryCreate(metadata, xml)
            }        
        let loadProjects (workingDir : DirectoryInfo) =
            FileSystem.ReadFilesMatching searchFilesPattern workingDir
            |> Seq.map ProjectMetadata.create
            |> Seq.choose tryParseProject
            |> Seq.toArray
        
        let! directory = workingDir |> FileSystem.TryGetDirectoryInfo        
        let projects = directory |> loadProjects
        let version = WorkspaceVersion.create projects
        let workspace = { Name = directory.Name
                          Directory = directory.FullName
                          Projects = projects
                          Version = version }
        return workspace
    }

let GetNextVersion (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
    option {
        let! propertyGroup = workspace.Version.PropertyGroup
        let! nextPropertyGroupVersion = Version.tryGetNext propertyGroup timeStamp |> Some
        return { PropertyGroup = nextPropertyGroupVersion }
    }