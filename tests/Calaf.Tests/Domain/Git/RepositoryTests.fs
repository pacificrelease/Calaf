namespace Calaf.Tests.RepositoryTests

open FsCheck.Xunit

open Calaf.Contracts
open Calaf.Domain
open Calaf.Domain.DomainTypes
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
    let ``Unborn GitRepositoryInfo produces unborn Repository with the corresponding event``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo = { gitRepositoryInfo with Directory = directory; Unborn = true }
        match tryCapture updatedRepoInfo with
        | Ok (Unborn path, events) ->
            path = directory &&
            events.Length = 1 &&
            events.Head =
                ({ Version = None; State = RepositoryState.Unborn }
                |> RepositoryEvent.StateCaptured
                |> DomainEvent.Repository)
        | _ -> false
        
    [<Property(Arbitrary = [| typeof<Arbitrary.directoryPathString> |])>]
    let ``None commit of GitRepositoryInfo produces unborn Repository with the corresponding event``
        (directory: string) (gitRepositoryInfo: GitRepositoryInfo)=
        let updatedRepoInfo =
            { gitRepositoryInfo with
                Directory = directory
                Unborn = false
                CurrentCommit = None }
        match tryCapture updatedRepoInfo with
        | Ok (Unborn path, events) ->
            path = directory &&
            events.Length = 1 &&
            events.Head =
                ({ Version = None; State = RepositoryState.Unborn }
                |> RepositoryEvent.StateCaptured
                |> DomainEvent.Repository)
        | _ -> false