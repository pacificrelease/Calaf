namespace Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Application

module internal ProjectsScope =        
    type private WorkspaceDirectoryPath = private WorkspaceDirectoryPath of string
    type private ProjectFullPath = private ProjectFullPath of string
    type private ProjectValidatedPath = private ProjectValidatedPath of ProjectFullPath
    
    let private tryGetWorkspaceDirectory
        (workspaceDirectoryPath: string) =
        try
            workspaceDirectoryPath
            |> System.IO.Path.GetFullPath
            |> WorkspaceDirectoryPath
            |> Ok
        with
        | exn ->
            workspaceDirectoryPath
            |> BadWorkingDirectoryPath
            |> CalafError.Validation
            |> Error
    let private tryGetProjectFullPath
        (workspaceDirectoryPath: WorkspaceDirectoryPath)
        (projectPath: string) =
            try
                let (WorkspaceDirectoryPath workspaceDirectoryPath) = workspaceDirectoryPath
                let projectFullPath =
                    if System.IO.Path.IsPathRooted projectPath
                    then System.IO.Path.GetFullPath projectPath
                    else System.IO.Path.GetFullPath(System.IO.Path.Combine(workspaceDirectoryPath, projectPath))
                ProjectFullPath projectFullPath |> Ok
            with
            | exn ->
                projectPath
                |> BadProjectPath
                |> CalafError.Validation
                |> Error            
            
    let private getRelativePath
        (workspaceDirectoryPath: WorkspaceDirectoryPath)
        (projectFullPath: ProjectFullPath) =
            let (WorkspaceDirectoryPath workspaceDirectory) = workspaceDirectoryPath
            let (ProjectFullPath projectFullPath) = projectFullPath
            System.IO.Path.GetRelativePath(workspaceDirectory, projectFullPath)
            
    let private tryValidate
        (workspaceDirectoryPath: WorkspaceDirectoryPath)
        (projectPath: string) =
        result {
            let! projectFullPath = tryGetProjectFullPath workspaceDirectoryPath projectPath
            let relativePath = getRelativePath workspaceDirectoryPath projectFullPath
            
            if relativePath.StartsWith("..") || System.IO.Path.IsPathRooted relativePath
            then
                return!
                    relativePath
                    |> RestrictedProjectPath
                    |> CalafError.Validation
                    |> Error
            else            
                return ProjectValidatedPath projectFullPath
        }
        
    let private tryCreateProjectsSearchScope
        (projectsValidatedPaths: ProjectValidatedPath list) =
        let tryClassify (ProjectValidatedPath (ProjectFullPath path)) =
            try
                let choice =
                    if System.IO.Path.HasExtension(path)
                    then Choice1Of2 path
                    else Choice2Of2 path
                Ok choice
            with
            | exn ->
                path
                |> BadProjectPath
                |> CalafError.Validation
                |> Error
                
        let partitionChoices choices =
           let files = choices |> List.choose (function Choice1Of2 f -> Some f | _ -> None)
           let dirs  = choices |> List.choose (function Choice2Of2 d -> Some d | _ -> None)
           (files, dirs)           
        
        result {
            let! choices = projectsValidatedPaths |> List.traverseResultM tryClassify
            let files, dirs = partitionChoices choices
            let! validatedFiles =
                files
                |> List.traverseResultM (fun f -> f |> ValidatedFileInfo |> Ok)
            let! validatedDirs =
                dirs
                |> List.traverseResultM (fun d -> d |> ValidatedDirectoryInfo |> Ok)
            return
                match validatedFiles, validatedDirs with
                | [], [] -> None
                | [], _  -> Some (DirectoriesOnly validatedDirs)
                | _, []  -> Some (FilesOnly validatedFiles)
                | _, _   -> Some (FilesAndDirectories (validatedDirs, validatedFiles))            
        }
            
    let tryCreate
        (directory: string)
        (projects: string list) =
        result {
            let! workspaceDirectory = tryGetWorkspaceDirectory directory
            let! projectsValidatedPaths =
                projects
                |> List.traverseResultM (tryValidate workspaceDirectory)
            let! scope = tryCreateProjectsSearchScope projectsValidatedPaths
            return scope
        }