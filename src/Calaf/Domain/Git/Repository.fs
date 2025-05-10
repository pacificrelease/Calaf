module internal Calaf.Domain.Repository

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (repoInfo: GitRepositoryInfo) =
    let create ctor (signature: GitSignatureInfo) (commit: GitCommitInfo) (branch: string option) =
        let signature = { Name = signature.Name
                          Email = signature.Email
                          When = signature.When }
        let head = Head.create repoInfo.Detached commit branch        
        let version = repoInfo.Tags
                   |> Seq.map Tag.create
                   |> Tag.chooseCalendarVersions        
                   |> Version.tryMax
        ctor (repoInfo.Directory, head, signature, version)
        
    match repoInfo with
    | { Damaged = true } ->
        Damaged repoInfo.Directory      
    | i when i.Unborn || i.CurrentCommit.IsNone ->
        Unborn repoInfo.Directory    
    | i when i.Dirty &&
             i.CurrentCommit.IsSome &&
             i.Signature.IsSome ->
        create Repository.Dirty i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
    | i when i.CurrentCommit.IsSome &&
             i.Signature.IsSome ->
        create Repository.Ready i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
    | i when i.Signature.IsNone ->
        Unsigned repoInfo.Directory
    | _ ->
        Damaged repoInfo.Directory