module Calaf.Guards
    open FsToolkit.ErrorHandling
    
    open Calaf.Application
    
    module Projects =
        let private getOperationSystemComparison =
            if System.OperatingSystem.IsWindows()
            then System.StringComparison.OrdinalIgnoreCase
            else System.StringComparison.Ordinal

        let private tryGetWorkspaceFullPath
            (workspace: string) =
            try
                workspace
                |> System.IO.Path.GetFullPath
                |> System.IO.Path.TrimEndingDirectorySeparator
                |> Ok
            with
            | _ ->
                workspace
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
                            pFull.StartsWith($"{wFull}{string System.IO.Path.DirectorySeparatorChar}", comparison)

                    if inside
                    then return pFull
                    else
                        return! project
                        |> RestrictedProjectPath
                        |> CalafError.Validation
                        |> Error
            }
            
        let check
            (directory: string)
            (projects: string list) =
            result {
                let! projectsValidatedPaths =
                    projects
                    |> List.traverseResultM (tryValidate directory)
                return projectsValidatedPaths
            }
            
    module Directory =        
        let check path =        
            if System.String.IsNullOrWhiteSpace path
            then "."
            else path