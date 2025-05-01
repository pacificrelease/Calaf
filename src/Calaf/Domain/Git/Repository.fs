module internal Calaf.Domain.Repository

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (repoInfo: GitRepositoryInfo) =
    let create ctor commit branch =
        let head = Head.create repoInfo.Detached commit branch        
        let version = repoInfo.Tags
                   |> Seq.map Tag.create
                   |> Tag.chooseCalendarVersions        
                   |> Version.tryMax
                   |> Option.map CalVer
        ctor (repoInfo.Directory, head, version)
        
    match repoInfo with
    | { Damaged = true } ->
        Damaged repoInfo.Directory        
    | i when i.Unborn || i.CurrentCommit.IsNone ->
        Unborn repoInfo.Directory        
    | i when i.Dirty && i.CurrentCommit.IsSome ->
        create Repository.Dirty i.CurrentCommit.Value i.CurrentBranch
    | i when i.CurrentCommit.IsSome ->
        create Repository.Ready i.CurrentCommit.Value i.CurrentBranch
    | _ ->
        Damaged repoInfo.Directory