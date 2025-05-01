module internal Calaf.Domain.Head

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (detached: bool) (commitInfo: GitCommitInfo) (branchName: string option) =
    let commit = Commit.create commitInfo
    match detached, branchName with
    | false, Some branchName -> Attached (commit, branchName)
    | _ -> Detached commit