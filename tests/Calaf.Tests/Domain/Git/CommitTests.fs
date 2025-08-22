namespace Calaf.Tests.CommitTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain.Commit
open Calaf.Tests

module CreatePropertiesTests =        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo> |])>]
    let ``Contract creates a corresponding domain value`` (contract: GitCommitInfo) =        
        let commit = create contract
        commit.Text = contract.Text &&
        commit.Hash = contract.Hash &&
        commit.When = contract.When