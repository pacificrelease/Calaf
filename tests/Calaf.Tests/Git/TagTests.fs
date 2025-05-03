namespace Calaf.Tests.TagTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.Tag
open Calaf.Tests

module CreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calVerGitTagInfo> |])>]
    let ``CalVer version tag creates Tag.Versioned with CalVer version`` (contract: GitTagInfo)  =
        match create contract with
        | Tag.Versioned(name, CalVer calVer, commitOption) ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.semVerGitTagInfo> |])>]
    let ``SemVer version tag creates Tag.Versioned with SemVer version`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Versioned(name, SemVer semVer, commitOption) ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.unversionedGitTagInfo> |])>]
    let ``No version tag creates Tag.Unversioned with the corresponding name`` (contract: GitTagInfo) =
        match create contract with
        | Tag.Unversioned name ->
            name = contract.Name
        | _ -> false

