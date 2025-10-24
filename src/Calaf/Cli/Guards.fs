module private Calaf.Guards
    open FsToolkit.ErrorHandling
    
    open Calaf.Application
    
    module Projects =
        let private getOperationSystemComparison =
            if System.OperatingSystem.IsWindows()
            then System.StringComparison.OrdinalIgnoreCase
            else System.StringComparison.Ordinal        

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
                
        let private tryGetProjectRelativePath
            (workspaceFullPath: string)
            (projectFullPath: string) =
                try
                    (workspaceFullPath, projectFullPath)
                    |> System.IO.Path.GetRelativePath
                    |> System.IO.Path.TrimEndingDirectorySeparator
                    |> Ok
                with
                | _ ->
                    projectFullPath
                    |> BadProjectPath
                    |> CalafError.Validation
                    |> Error

        let private tryValidate
            (workspace: ValidatedDirectoryFullPath)
            (project: string) =
            result {
                try                    
                    let (ValidatedDirectoryFullPath workspace) = workspace
                    let! pFull = tryGetProjectFullPath workspace project
                    
                    let wRoot = System.IO.Path.GetPathRoot workspace
                    let pRoot = System.IO.Path.GetPathRoot pFull
                    let comparison = getOperationSystemComparison

                    if not (System.String.Equals(wRoot, pRoot, comparison))
                    then
                        return! project
                        |> RestrictedProjectPath
                        |> CalafError.Validation
                        |> Error
                    else
                        let wIsRoot = System.String.Equals(workspace, wRoot, comparison)
                        let inside =
                            if wIsRoot
                            then true
                            else
                                pFull.Equals(workspace, comparison) ||
                                pFull.StartsWith($"{workspace}{string System.IO.Path.DirectorySeparatorChar}", comparison)
                        if inside
                        then
                            let! pRelative = tryGetProjectRelativePath workspace pFull
                            return {
                                ValidatedFullPath = pFull
                                ValidatedRelativePath = pRelative
                            }
                        else
                            return!
                                project
                                |> RestrictedProjectPath
                                |> CalafError.Validation
                                |> Error
                with
                | _ ->
                    return!
                        project
                        |> BadProjectPath
                        |> CalafError.Validation
                        |> Error
            }
            
        let check
            (directory: ValidatedDirectoryFullPath)
            (projects: string list) =
            result {
                return!
                    projects
                    |> List.traverseResultM (tryValidate directory)
            }
            
    module Workspace =        
        let getPathOrDefault directory =        
            if System.String.IsNullOrWhiteSpace directory
            then "."
            else directory
            
        let check
            directory =
            try
                directory
                |> System.IO.Path.GetFullPath
                |> System.IO.Path.TrimEndingDirectorySeparator
                |> ValidatedDirectoryFullPath
                |> Ok
            with
            | _ ->
                directory
                |> BadWorkspacePath
                |> CalafError.Validation
                |> Error