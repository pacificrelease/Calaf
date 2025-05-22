namespace Calaf.Tests.RepositoryTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities
open Calaf.Domain.DomainEvents
open Calaf.Domain.Repository
open Calaf.Tests

module TryCreatePropertiesTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.nullOrWhiteSpaceString> |])>]
    let ``Any GitRepositoryInfo with empty, null or whitespace path produces RepositoryPathEmpty error``
        (emptyOrWhitespaceString: string) (gitRepositoryInfo: GitRepositoryInfo) =
        let gitRepositoryInfo = 
            { gitRepositoryInfo with Directory = emptyOrWhitespaceString }
        tryCapture gitRepositoryInfo = Error RepositoryPathEmpty
        
    [<Property(Arbitrary = [| typeof<Arbitrary.directoryPathString> |])>]
    let ``Damaged GitRepositoryInfo produces damaged Repository with the corresponding event``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo = { gitRepositoryInfo with Directory = directory; Damaged = true }
        match tryCapture updatedRepoInfo with
        | Ok (Damaged path, events) ->
            path = directory &&
            events.Length = 1 &&
            events.Head = (RepositoryCaptured { Version = None; State = RepositoryState.Damaged } |> DomainEvent.Repository)
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.directoryPathString> |])>]
    let ``Unborn GitRepositoryInfo produces unborn Repository with the corresponding event``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo = { gitRepositoryInfo with Directory = directory; Damaged = false; Unborn = true }
        match tryCapture updatedRepoInfo with
        | Ok (Unborn path, events) ->
            path = directory &&
            events.Length = 1 &&
            events.Head = (RepositoryCaptured { Version = None; State = RepositoryState.Unborn } |> DomainEvent.Repository)
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.directoryPathString> |])>]
    let ``None commit of GitRepositoryInfo produces unborn Repository with the corresponding event``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo =
            { gitRepositoryInfo with
                Directory = directory
                Damaged = false
                Unborn = false
                CurrentCommit = None }
        match tryCapture updatedRepoInfo with
        | Ok (Unborn path, events) ->
            path = directory &&
            events.Length = 1 &&
            events.Head = (RepositoryCaptured { Version = None; State = RepositoryState.Unborn } |> DomainEvent.Repository)
        | _ -> false