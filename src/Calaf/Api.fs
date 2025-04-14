// Api. Composition Root
namespace Calaf

open System.IO
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain

module Api =
    let CreateWorkspace workingDir searchFilesPattern : Result<Workspace, CalafError> =
        result {             
            let tryParseProject (metadata: ProjectMetadata) : Project option =
                option {                
                    let result = Xml.TryLoadXml(metadata.AbsolutePath)
                    match result with
                    | Error _ ->
                        // Handle error or mb add new type of the Project -> Unavailable (for example)
                        return! None
                    | Ok xml ->
                        let! project = Project.tryCreate(metadata, xml)
                        return project
                }        
            let loadProjects (workingDir : DirectoryInfo) =
                FileSystem.ReadFilesMatching searchFilesPattern workingDir
                |> Seq.map ProjectMetadata.create
                |> Seq.choose tryParseProject
                |> Seq.toArray
            
            let! directory = workingDir |> FileSystem.TryGetDirectoryInfo |> Result.mapError CalafError.FileSystem
            let projects = directory |> loadProjects
            return Workspace.create(directory, projects)
        }
        
    let BumpWorkspace (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
        option {
            let! propertyGroup = workspace.Version.PropertyGroup
            let! nextPropertyGroupVersion = Version.tryBump propertyGroup timeStamp |> Some
            return { PropertyGroup = nextPropertyGroupVersion }
        }    

    // TODO: Remove this function because of another workflows is going to be used
    let GetNextVersion (workspace: Workspace) (timeStamp: System.DateTime) : WorkspaceVersion option =
        option {
            let! propertyGroup = workspace.Version.PropertyGroup
            let! nextPropertyGroupVersion = Version.tryBump propertyGroup timeStamp |> Some
            return { PropertyGroup = nextPropertyGroupVersion }
        }