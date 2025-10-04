namespace Calaf.Application

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Application

module internal ProjectsScope =    
    let private tryGetWorkspaceFullPath
        (workspacePath: string) =
        try
            workspacePath
            |> System.IO.Path.GetFullPath
            |> System.IO.Path.TrimEndingDirectorySeparator
            |> Ok
        with
        | _ ->
            workspacePath
            |> BadWorkspacePath
            |> CalafError.Validation
            |> Error
            
    let private tryGetProjectFullPath
        (workspace: string)
        (project: string) =
            try
                let p =
                    if System.IO.Path.IsPathFullyQualified project
                    then System.IO.Path.GetFullPath project
                    else
                        System.IO.Path.Combine(workspace, project)
                        |> System.IO.Path.GetFullPath
                System.IO.Path.TrimEndingDirectorySeparator p |> Ok
            with
            | _ ->
                project
                |> BadProjectPath
                |> CalafError.Validation
                |> Error
                
    let private getOperationSystemComparison =
        if System.OperatingSystem.IsWindows()
        then System.StringComparison.OrdinalIgnoreCase
        else System.StringComparison.Ordinal 
            
    let private tryValidate
        (workspace: string)
        (project: string) =
        result {
            let! wFull = tryGetWorkspaceFullPath workspace
            let! pFull = tryGetProjectFullPath wFull project        

            
            let wRoot = System.IO.Path.GetPathRoot wFull
            let pRoot = System.IO.Path.GetPathRoot pFull
            let comparison = getOperationSystemComparison

            if not (System.String.Equals(wRoot, pRoot, comparison))
            then
                return! project
                |> RestrictedProjectPath
                |> CalafError.Validation
                |> Error
            else
                let wIsRoot = System.String.Equals(wFull, wRoot, comparison)
                let inside =
                    if wIsRoot
                    then true
                    else
                        pFull.Equals(wFull, comparison) ||
                        //pFull.StartsWith(wFull + string System.IO.Path.DirectorySeparatorChar, comparison)
                        pFull.StartsWith($"{wFull}{string System.IO.Path.DirectorySeparatorChar}", comparison)

                if inside
                then return pFull
                else
                    return! project
                    |> RestrictedProjectPath
                    |> CalafError.Validation
                    |> Error
        }
        
    let private tryCreateProjectsSearchScope
        (projectsValidatedPaths: string list) =
        let tryClassify (path: string) =
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
            let! workspaceDirectory = tryGetWorkspaceFullPath directory
            let! projectsValidatedPaths =
                projects
                |> List.traverseResultM (tryValidate workspaceDirectory)
            let! scope = tryCreateProjectsSearchScope projectsValidatedPaths
            return scope
        }