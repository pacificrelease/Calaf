namespace Calaf.Tests.RepositoryTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes
open Calaf.Domain.Repository
open Calaf.Tests

module TryCreatePropertiesTests =
    // Empty path GitRepositoryInfo produces EmptyRepositoryPath error    
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Any GitRepositoryInfo with empty, null or whitespace path produces EmptyRepositoryPath error``
        (emptyOrWhitespaceString: string) (gitRepositoryInfo: GitRepositoryInfo) =
        let gitRepositoryInfo = 
            { gitRepositoryInfo with Directory = emptyOrWhitespaceString }
        tryCreate gitRepositoryInfo = Error EmptyRepositoryPath
        
    // Damaged GitRepositoryInfo produces DamagedRepository error
    [<Property(Arbitrary = [| typeof<Arbitrary.absoluteOrRelativePathString> |])>]
    let ``Damaged GitRepositoryInfo produces damaged Repository``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo = { gitRepositoryInfo with Directory = directory; Damaged = true }
        match tryCreate updatedRepoInfo with
        | Ok (Damaged path) -> path = directory
        | _ -> false