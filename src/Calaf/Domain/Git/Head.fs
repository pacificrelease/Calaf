module internal Calaf.Domain.Head

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values

let tryCreate (detached: bool) (commitInfo: GitCommitInfo) (branchName: string option) =
    let commit = Commit.create commitInfo    
    match detached, branchName with
    | false, Some branchName when System.String.IsNullOrWhiteSpace branchName    
        -> EmptyBranchName |> Error
    | false, Some branchName
        -> Attached (commit, branchName) |> Ok    
    | _ -> Detached commit |> Ok