namespace Calaf.Tests.TagTests

open Calaf.Domain.DomainTypes
open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain.Tag
open Calaf.Tests

module CreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.Git.calVerGitTagInfo> |])>]
    let ``Valid CalVer version tag creates Tag.Versioned with CalVer version`` (contract: GitTagInfo)  =
        match create contract with
        | Tag.Versioned(name, version, commitOption) ->
            name = contract.Name &&
            Option.isSome contract.Commit = Option.isSome commitOption &&
            version.IsCalVer
        | _ -> false
        

