namespace Calaf.Tests.HeadTests

open FsCheck.Xunit
    
open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.Head
open Calaf.Tests

module CreatePropertiesTests =    
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo>; typeof<Arbitrary.Git.branchName> |])>]
    let ``Non-detached Head and branch creates the Attached Head with the corresponding values``
        (gitCommitInfo: GitCommitInfo, branchName: string) =
        let detachedHead = false 
        let head = tryCreate detachedHead gitCommitInfo (Some branchName)
        match head with        
        | Ok (Attached (commit, branch)) ->
            commit.Text = gitCommitInfo.Text &&
            commit.Hash = gitCommitInfo.Hash &&
            commit.When = gitCommitInfo.When &&
            branch = branchName
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo>; typeof<Arbitrary.Git.branchName> |])>]
    let ``Detached Head with branch creates Detached Head with the corresponding values``
        (gitCommitInfo: GitCommitInfo, branchName: string) =
        let detachedHead = true 
        let head = tryCreate detachedHead gitCommitInfo (Some branchName)
        match head with        
        | Ok (Detached commit) ->
            commit.Text = gitCommitInfo.Text &&
            commit.Hash = gitCommitInfo.Hash &&
            commit.When = gitCommitInfo.When
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo> |])>]
    let ``Non-detached Head without a branch creates the Detached Head with the corresponding commit``
        (gitCommitInfo: GitCommitInfo) =
        let detachedHead = false 
        let head = tryCreate detachedHead gitCommitInfo None
        match head with        
        | Ok (Detached commit) ->
            commit.Text = gitCommitInfo.Text &&
            commit.Hash = gitCommitInfo.Hash &&
            commit.When = gitCommitInfo.When
        | _ -> false        
    
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo>; typeof<Arbitrary.Git.branchName> |])>]
    let ``Head creation provides corresponding commit data``
        (detachedHead: bool, gitCommitInfo: GitCommitInfo, branchName: string, shouldOptional: bool) =
        let branchName = if shouldOptional then Some branchName else None
        match tryCreate detachedHead gitCommitInfo branchName with
        | Ok head ->
            let commit =
                match head with
                | Detached c
                | Attached (c, _) -> c
            commit.Text = gitCommitInfo.Text &&
            commit.Hash = gitCommitInfo.Hash &&
            commit.When = gitCommitInfo.When
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.gitCommitInfo>; typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Non-detached Head creation with empty/null/whitespace branch name provides BranchNameEmpty error``
        (gitCommitInfo: GitCommitInfo, emptyBranchName: string) =
        let detachedHead = false
        tryCreate detachedHead gitCommitInfo (Some emptyBranchName) = (DomainError.BranchNameEmpty |> Error)