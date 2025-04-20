// Api. Composition Root
namespace Calaf

open System.IO
open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain

module Api =
    [<Literal>]
    let private supportedDotNetFilesPattern = "*.?sproj"
    
    let CreateWorkspace workingDir : Result<Workspace, CalafError> =
        result {             
            let tryParseProject (metadata: ProjectMetadata) : Project option =
                option {                
                    let xmlResult = Xml.TryLoadXml(metadata.AbsolutePath)
                    match xmlResult with
                    | Error _ ->
                        // Handle error or mb add new type of the Project -> Unavailable (for example)
                        return! None
                    | Ok xml ->
                        let! project = Project.tryCreate(metadata, xml)
                        return project
                }        
            let loadProjects (workingDir : DirectoryInfo) =
                result {
                    let! xmlFiles = FileSystem.TryReadPatternFiles supportedDotNetFilesPattern workingDir
                    return xmlFiles 
                        |> Seq.map ProjectMetadata.create
                        |> Seq.choose tryParseProject
                        |> Seq.toArray
                }
                
            
            let! directory = workingDir |> FileSystem.TryGetDirectoryInfo |> Result.mapError CalafError.FileSystem
            let! projects = directory |> loadProjects |> Result.mapError CalafError.FileSystem
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
            let nextPropertyGroupVersion = Version.tryBump propertyGroup timeStamp
            return { PropertyGroup = nextPropertyGroupVersion }
        }