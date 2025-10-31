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
            
    let private tryDestructure
        (commands: MakeCommand2 list) =
        let destruct
            (flags: ParseResults<MakeFlag2>) =
            let changelog = flags.Contains Changelog
            let includePreRelease = flags.Contains IncludePreRelease
            let targetProjects =
                flags.TryGetResult Projects
                |> Option.defaultValue List.Empty
                |> List.filter (fun p ->
                    not (System.String.IsNullOrWhiteSpace p))
                |> List.distinct
            if includePreRelease && not changelog then
                ChangelogFlagRequired
                |> Input
                |> CalafError.Infrastructure
                |> Error
            else
                let destructuredArguments = {|
                    Changelog = changelog
                    IncludePreRelease = includePreRelease
                    TargetProjects = targetProjects |}
                Ok destructuredArguments
                
        let combine
            releaseType
            flags =
            destruct flags
            |> Result.map (fun a ->
                {| ReleaseType = releaseType
                   Changelog = a.Changelog
                   IncludePreRelease = a.IncludePreRelease
                   TargetProjects = a.TargetProjects |})
            
        match commands with
            | [ MakeCommand2.Nightly nFlags ] -> combine ReleaseType.Nightly nFlags
            | [ MakeCommand2.Alpha aFlags ] -> combine ReleaseType.Alpha aFlags
            | [ MakeCommand2.Beta bFlags ] -> combine ReleaseType.Beta bFlags
            | [ MakeCommand2.RC rcFlags ] -> combine ReleaseType.RC rcFlags
            | [ MakeCommand2.Stable sFlags ] -> combine ReleaseType.Stable sFlags
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
                
    let private tryTargetProjectSpec
        (workspace: ValidatedDirectoryFullPath)
        (project: ValidatedProjectFullPath)=
        let (ValidatedDirectoryFullPath workspace) = workspace
        let (ValidatedProjectFullPath project) = project
        try
            let relativePath =
                (workspace, project)
                |> System.IO.Path.GetRelativePath
            Ok { AbsolutePath = project
                 RelativePath = relativePath }
        with
        | _ ->
            BadProjectPath project
            |> CalafError.Validation
            |> Error
    
    // let private trySpec
    //     (directory: ValidatedDirectoryFullPath)
    //     (args: ParseResults<InputCommand2>) =
    //     result {            
    //         let cliArgs = args.GetAllResults()
    //         match cliArgs with
    //         | [ Make2 makeArgs ] ->
    //             return!
    //                 makeArgs.GetAllResults()
    //                 |> tryDestructure
    //                 |> Result.bind (fun a ->
    //                     result {
    //                         let! projects = Guards.Projects.check directory a.TargetProjects
    //                         let (ValidatedDirectoryFullPath directory) = directory
    //                         let projects =
    //                             projects                                
    //                             |> List.map (fun x ->
    //                                 { FullPath = x
    //                                   RelativePath = System.IO.Path.GetRelativePath(directory, x) })                               
    //                         
    //                         let targetProjects =
    //                             if projects.IsEmpty
    //                             then None
    //                             else Some projects
    //                         let changelogGeneration =
    //                             if changelog
    //                             then Some { IncludePreRelease = includePreRelease }
    //                             else None
    //                         
    //                         let makeSpec : MakeSpec =
    //                             { WorkspaceDirectory = directory
    //                               VersionType = versionType
    //                               ChangelogGeneration = changelogGeneration
    //                               TargetProjects = targetProjects }
    //                         return makeSpec                            
    //                     })
    //         | [] ->
    //             let makeSpec : MakeSpec =
    //                let (ValidatedDirectoryFullPath directory) = directory
    //                { WorkspaceDirectory = directory
    //                  VersionType = VersionType.Stable
    //                  ChangelogGeneration = Some { IncludePreRelease = true }
    //                  TargetProjects = None }
    //             return makeSpec                                              
    //         | commands ->
    //             return!
    //                 ($"{commands.Head}" |> CommandNotRecognized |> Input |> CalafError.Infrastructure)
    //                 |> Error
    //     }
                
    // let tryParseToSpec (directory: string) (args: string[]) =
    //     result {
    //         let! workspace = tryWorkspace directory |> Result.mapError CliError.map
    //         let! commands = tryCommands args |> Result.mapError CliError.map
    //         let! spec =
    //             trySpec workspace commands
    //             |> Result.mapError CliError.map
    //         return spec
    //     }