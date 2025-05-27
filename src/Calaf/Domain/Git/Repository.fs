module internal Calaf.Domain.Repository

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities
open Calaf.Domain.DomainEvents

module Events =
    let private toState = function
        | Damaged  _ -> RepositoryState.Damaged
        | Dirty    _ -> RepositoryState.Dirty
        | Unborn   _ -> RepositoryState.Unborn
        | Unsigned _ -> RepositoryState.Unsigned
        | Ready    _ -> RepositoryState.Ready
        
    let toRepositoryCaptured repo =
        let state = toState repo
        let version =
            match repo with
            | Dirty (_, _, _, calendarVersion)
            | Ready (_, _, _, calendarVersion) -> Option.map CalVer calendarVersion
            | _ -> None
        RepositoryCaptured { Version = version; State = state } |> DomainEvent.Repository
        
    let toRepositoryBumped repo version signature =
        let state = toState repo        
        RepositoryBumped {
            Version = version
            Signature = signature
            State = state
        } |> DomainEvent.Repository

let tryCapture (repoInfo: GitRepositoryInfo) =
    let tryValidatePath path =
        if not (System.String.IsNullOrWhiteSpace path)
        then Ok path
        else RepositoryPathEmpty |> Error
    result {
        let tryCreate ctor (signature: GitSignatureInfo) (commit: GitCommitInfo) (branch: string option) =
            result {
                let signature = { Name = signature.Name
                                  Email = signature.Email
                                  When = signature.When }
                let! head = Head.tryCreate repoInfo.Detached commit branch
                let version = repoInfo.Tags
                           |> Seq.map Tag.create
                           |> Tag.chooseCalendarVersions        
                           |> Version.tryMax
                return ctor (repoInfo.Directory, head, signature, version)
            }
        let! path = tryValidatePath repoInfo.Directory
        match repoInfo with
        | { Damaged = true } ->
            let repo = Damaged path
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
        | i when i.Unborn || i.CurrentCommit.IsNone ->
            let repo = Unborn path
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
        | i when i.Dirty &&
                 i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            let! repo = tryCreate Repository.Dirty i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
        | i when i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            let! repo = tryCreate Repository.Ready i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
        | i when i.Signature.IsNone ->
            let repo = Unsigned path
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
        | _ ->
            let repo = Damaged path
            let event = Events.toRepositoryCaptured repo
            return (repo, [event])
    }    
    
let tryGetCalendarVersion repo =
    match repo with
    | Ready (_, _, _, version) -> version
    | Dirty (_, _, _, version) -> version
    | _ -> None
    
let tryProfile (repo: Repository) (pendingFilesPaths: string list) =    
    match repo with        
    | Ready (dir, _, signature, Some currentVersion)        
    | Dirty (dir, _, signature, Some currentVersion) ->
        let tagName = Version.toTagName currentVersion
        let commitMessage = Version.toCommitMessage currentVersion
        Some { Directory = dir
               Files = pendingFilesPaths
               Signature = signature
               TagName = tagName
               CommitMessage = commitMessage }
    | _ -> None    
    
let tryBump (repo: Repository) (nextVersion: CalendarVersion) =
    let performBump (ctor, dir, head, signature, currentVersion) =
        let sameVersion = currentVersion
                            |> Option.map ((=) nextVersion)
                            |> Option.exists id
        if sameVersion
        then
            Error RepositoryAlreadyCurrent
        else
            let repo = ctor (dir, head, signature, Some nextVersion)
            let event = Events.toRepositoryBumped repo nextVersion signature
            Ok (repo, [event])
    
    match repo with        
    | Ready (dir, head, signature, currentVersion) ->
        performBump (Repository.Ready, dir, head, signature, currentVersion)
    | Dirty (dir, head, signature, currentVersion) ->
        performBump (Repository.Dirty, dir, head, signature, currentVersion)
    | Unborn   _ -> RepositoryHeadUnborn |> Error
    | Unsigned _ -> RepositoryUnsigned   |> Error
    | Damaged  _ -> RepositoryCorrupted  |> Error