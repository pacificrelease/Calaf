module internal Calaf.Domain.Git

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let createCommit (info: GitCommitInfo) =
    { Message = info.Message
      Hash = info.Hash
      When = info.When } 

let createTag (info: GitTagInfo) =
    match Version.tryParseFromTag info.Name with
    | Some version ->
        let commit = info.Commit |> Option.map createCommit
        Tag.Versioned (info.Name, version, commit)
    | None ->
        Tag.Unversioned info.Name
// let createTag { Name = name; Commit = commitInfo } =
//     Version.tryParseFromTag name
//     |> Option.map (fun version -> 
//         Tag.Versioned(name, version, Option.map createCommit commitInfo))
//     |> Option.defaultValue (Tag.Unversioned name)

let createHead detached commit branch =
    let commit = createCommit commit
    match detached, branch with
    | false, Some branchName -> Attached (commit, branchName)
    | _ -> Detached commit
    
let chooseCalendarVersions (tags: Tag seq) : CalendarVersion seq =
    tags
    |> Seq.choose (function
        | Tag.Versioned (_, CalVer version, _) -> Some version
        | _ -> None)

let create (info: GitRepositoryInfo) =
    let create repoConstructor directory detached commit branch tags =
        let head = createHead detached commit branch        
        let version = info.Tags
                   |> Seq.map createTag
                   |> chooseCalendarVersions        
                   |> Version.tryMax
                   |> Option.map CalVer
        repoConstructor(directory, head, version)
        
    match info with
    | { Damaged = true } ->
        Damaged info.Directory
        
    | i when i.Unborn || i.CurrentCommit.IsNone ->
        Unborn info.Directory
        
    | i when i.Dirty && i.CurrentCommit.IsSome ->
        // let head = createHead i.Detached i.CurrentCommit.Value i.CurrentBranch
        // let tags = info.Tags |> Array.map createTag
        // Dirty (info.Directory, head, tags)
        create Repository.Dirty i.Directory i.Detached i.CurrentCommit.Value i.CurrentBranch info.Tags
        
    | i when i.CurrentCommit.IsSome ->
        // let head = createHead i.Detached i.CurrentCommit.Value i.CurrentBranch
        // let tags = info.Tags |> Array.map createTag
        create Repository.Ready i.Directory i.Detached i.CurrentCommit.Value i.CurrentBranch info.Tags
    | _ ->
        Damaged info.Directory