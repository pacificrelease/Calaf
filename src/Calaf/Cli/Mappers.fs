module Calaf.Arguments
    open Argu
    open FsToolkit.ErrorHandling

    open Calaf.Application
    
    let private tryWorkspace (directory: string) =
        directory
        |> Guards.Workspace.getPathOrDefault
        |> Guards.Workspace.check

    let private tryCommands (args: string[]) =
        try
            ArgumentParser.Create<InputCommand2>().ParseCommandLine(args) |> Ok
        with
        | exn ->
            exn.Message
            |> ArgumentsFatal
            |> Input
            |> CalafError.Infrastructure
            |> Error
            
    let private tryDestructure (commands: MakeCommand2 list) =
        let destruct (flags: ParseResults<MakeFlag2>) =
            let changelog = flags.Contains Changelog
            let includePreRelease = flags.Contains IncludePreRelease
            let targetProjects =
                flags.TryGetResult Projects
                |> Option.defaultValue List.Empty
                |> List.filter (fun p -> not (System.String.IsNullOrWhiteSpace p))
                |> List.distinct            
            if includePreRelease && not changelog then
                ChangelogFlagRequired
                |> Input
                |> CalafError.Infrastructure
                |> Error
            else
                Ok (changelog, includePreRelease, targetProjects)
                
        let combine versionType flags =
            destruct flags
            |> Result.map (fun (changelog, includePreRelease, targetProjects) -> 
                (versionType, changelog, includePreRelease, targetProjects))
            
        match commands with
            | [ MakeCommand2.Nightly nFlags ] -> combine VersionType.Nightly nFlags
            | [ MakeCommand2.Alpha aFlags ] -> combine VersionType.Alpha aFlags
            | [ MakeCommand2.Beta bFlags ] -> combine VersionType.Beta bFlags
            | [ MakeCommand2.RC rcFlags ] -> combine VersionType.ReleaseCandidate rcFlags
            | [ MakeCommand2.Stable sFlags ] -> combine VersionType.Stable sFlags
            | [] ->               
                MakeCommandMissing
                |> Input
                |> CalafError.Infrastructure
                |> Error
            | _  ->
                $"{commands.Head}"
                |> MakeCommandNotRecognized
                |> Input
                |> CalafError.Infrastructure
                |> Error
                
    let private tryTargetProject
        (directory: ValidatedDirectoryFullPath)
        (project: string) =
        // Get absolute and relative paths
        let absPath = System.IO.Path.GetFullPath(project)
        let (ValidatedDirectoryFullPath directory) = directory
        let relPath = System.IO.Path.GetRelativePath(System.IO.Directory.GetCurrentDirectory(), absPath)
        Ok (absPath, relPath)

    let private trySpec
        (directory: ValidatedDirectoryFullPath)
        (args: ParseResults<InputCommand2>) =
        result {            
            let cliArgs = args.GetAllResults()
            match cliArgs with
            | [ Make2 makeArgs ] ->
                return!
                    makeArgs.GetAllResults()
                    |> tryDestructure
                    |> Result.bind (fun (versionType, changelog, includePreRelease, projects) ->
                        result {
                            let! projects = Guards.Projects.check directory projects
                            let (ValidatedDirectoryFullPath directory) = directory
                            let targetProjects =
                                if projects.IsEmpty
                                then None
                                else Some projects
                            let changelogGeneration =
                                if changelog
                                then Some { IncludePreRelease = includePreRelease }
                                else None
                            let makeSpec : MakeSpec =
                                { WorkspaceDirectory = directory
                                  VersionType = versionType
                                  ChangelogGeneration = changelogGeneration
                                  TargetProjects = targetProjects }
                            return makeSpec                            
                        })
            | [] ->
                let makeSpec : MakeSpec =
                   let (ValidatedDirectoryFullPath directory) = directory
                   { WorkspaceDirectory = directory
                     VersionType = VersionType.Stable
                     ChangelogGeneration = Some { IncludePreRelease = true }
                     TargetProjects = None }
                return makeSpec                                              
            | commands ->
                return!
                    ($"{commands.Head}" |> CommandNotRecognized |> Input |> CalafError.Infrastructure)
                    |> Error
        }
                
    let tryParseToSpec (directory: string) (args: string[]) =
        result {
            let! workspace = tryWorkspace directory |> Result.mapError CliError.map
            let! commands = tryCommands args |> Result.mapError CliError.map
            let! spec =
                trySpec workspace commands
                |> Result.mapError CliError.map
            return spec
        }