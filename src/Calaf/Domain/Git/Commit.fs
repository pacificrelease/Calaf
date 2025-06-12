module internal Calaf.Domain.Commit

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values

let create (commitInfo: GitCommitInfo) =
    { Message = commitInfo.Message
      Hash = commitInfo.Hash
      When = commitInfo.When }