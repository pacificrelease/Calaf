module Calaf.Application.UseCase.Make
//
// open FsToolkit.ErrorHandling
//
// open Calaf.Domain
// open Calaf.Domain.DomainTypes
// open Calaf.Application
//
// let private computeTagsExclude (withChangelog, includePre) =
//         match withChangelog, includePre with
//         | true,  true  -> Some Version.preReleases
//         | true,  false -> Some List.Empty
//         | false, _     -> None
//
// let internal execute (spec: MakeSpec) (deps: Deps) : Result<Workspace, CalafError> = result {
//     let timeStamp = deps.UtcNow()
//     let policy = Policy.defaultPolicy
//
//     let! dir = deps.TryReadDirectory spec.WorkspaceDirectory [ policy.SearchPattern ] policy.ChangelogFileName
//     let tagsInclude = Version.versionPrefixes
//     let tagsExclude = computeTagsExclude (spec.WithChangelog, spec.IncludePreRelease)
//
//     let! repo = deps.TryGetRepo spec.WorkspaceDirectory policy.TagsTake tagsInclude tagsExclude timeStamp
//
//     let! workspace, _ =
//         Workspace.tryCapture (dir, repo)
//         |> Result.mapError CalafError.Domain
//
//     let! nextVersion =
//         let make =
//             match spec.VersionType with
//             | VersionType.Nightly -> Version.tryNightly
//             | VersionType.Alpha -> Version.tryAlpha
//             | VersionType.Beta -> Version.tryBeta
//             | VersionType.ReleaseCandidate -> Version.tryReleaseCandidate
//             | VersionType.Stable -> Version.tryStable
//         make workspace.Version timeStamp |> Result.mapError CalafError.Domain
//
//     let! commitsOpt =
//         match spec.ChangelogGeneration with
//         | Some { IncludePreRelease = _ } ->        
//             match workspace.Repository with
//             | Some (Dirty (_, { BaselineVersion = Some { TagName = tag; Version = CalVer _ } }))
//             | Some (Ready (_, { BaselineVersion = Some { TagName = tag; Version = CalVer _ } })) ->
//                 deps.TryListCommits workspace.Directory (Some tag) |> Result.map Some
//             | Some (Dirty (_, { BaselineVersion = None }))
//             | Some (Ready (_, { BaselineVersion = None })) ->
//                 deps.TryListCommits workspace.Directory None |> Result.map Some
//             | _ -> Ok None
//         |_ -> Ok None
//
//     let changesetOpt =
//         commitsOpt
//         |> Option.bind (fun commits ->
//             ReleaseNotes.tryCreate (commits |> List.map Commit.create) nextVersion timeStamp)
//
//     let! workspace', _ =
//         nextVersion |> Workspace.tryRelease workspace |> Result.mapError CalafError.Domain
//
//     let snapshot = Workspace.snapshot workspace' (changesetOpt |> Option.map fst)
//
//     do! snapshot.Changelog
//         |> Option.map (fun s -> deps.TryWriteMarkdown (s.AbsolutePath, s.ReleaseNotesContent))
//         |> Option.defaultValue (Ok ())
//
//     do! snapshot.Projects
//         |> List.traverseResultM (fun s -> deps.TryWriteXml (s.AbsolutePath, s.Content))
//         |> Result.map ignore
//
//     do! snapshot.Repository
//         |> Option.map (fun s -> deps.TryCreateCommit (s.Directory, s.PendingFilesPaths) s.CommitText s.TagName)
//         |> Option.defaultValue (Ok ())
//
//     return workspace'
// }