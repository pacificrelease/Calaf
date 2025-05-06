// Api. Composition Root
namespace Calaf

// module Runner =
//         
//     let initWorkspace dir =                
//         result {
//             let! dir     = dir |> FileSystem.tryGetDirectory
//             let! repo = dir |> Git.tryReadRepository
//             let! files   = dir |> FileSystem.tryReadFiles supportedFilesPattern
//             
//             let repo = Api.Git.init repo
//             let lPprojects,
//                 lErrors = files
//                 |> Seq.map Api.Project.load
//                 |> Result.partition
//             
//             let! currentVer =
//                 lPprojects
//                 |> Seq.map fst
//                 |> WorkspaceVersion.create
//                 |> fun x -> x.PropertyGroup |> Option.toResult (NoPropertyGroupWorkspaceVersion |> Api)                                
//             let timeStamp = Clock.now()
//             let! bumpedVer =
//                 Version.tryBump currentVer timeStamp.UtcDateTime
//                 |> Option.toResult (NoPropertyGroupNextVersion |> Api)
//             let bProjects,
//                 bErrors = lPprojects
//                 |> Project.choosePending
//                 |> Seq.map (fun pair -> Api.Project.bump pair bumpedVer)
//                 |> Result.partition
//                             
//             let sProjects,
//                 sErrors =  bProjects
//                 |> Seq.map Api.Project.save
//                 |> Result.partition
//
//             return Workspace.create (dir, sProjects |> Seq.map fst)
//         }