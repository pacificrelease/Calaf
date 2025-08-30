module internal Calaf.Domain.Commit

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (commitInfo: GitCommitInfo) =
    let message = CommitMessage.create commitInfo.Text
    { Message = message
      Text = commitInfo.Text
      Hash = commitInfo.Hash
      When = commitInfo.When }