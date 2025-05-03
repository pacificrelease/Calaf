namespace Calaf.Tests.CommitTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.Commit
open Calaf.Tests

module CreatePropertiesTests =        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo> |])>]
    let ``Contract creates a corresponding domain value`` (contract: GitCommitInfo) =        
        let commit = create contract
        commit.Message = contract.Message &&
        commit.Hash = contract.Hash &&
        commit.When = contract.When