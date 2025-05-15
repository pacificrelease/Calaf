module internal Calaf.Domain.Repository

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

let toState = function
    | Damaged  _ -> RepositoryState.Damaged
    | Dirty    _ -> RepositoryState.Dirty
    | Unborn   _ -> RepositoryState.Unborn
    | Unsigned _ -> RepositoryState.Unsigned
    | Ready    _ -> RepositoryState.Ready
    
let toRepositoryCreated repo =
    let state = toState repo
    let version =
        match repo with
        | Dirty (_, _, _, calendarVersion)
        | Ready (_, _, _, calendarVersion) -> Option.map CalVer calendarVersion
        | _ -> None
    RepositoryCreated { Version = version; State = state } |> DomainEvent.Repository

let tryCreate (repoInfo: GitRepositoryInfo) =
    let tryValidatePath path =
        if not (System.String.IsNullOrWhiteSpace path)
        then Ok path
        else EmptyRepositoryPath |> Error
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
            let event = toRepositoryCreated repo
            return (repo, [event])
        | i when i.Unborn || i.CurrentCommit.IsNone ->
            let repo = Unborn path
            let event = toRepositoryCreated repo
            return (repo, [event])
        | i when i.Dirty &&
                 i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            let! repo = tryCreate Repository.Dirty i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
            let event = toRepositoryCreated repo
            return (repo, [event])
        | i when i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            let! repo = tryCreate Repository.Ready i.Signature.Value i.CurrentCommit.Value i.CurrentBranch
            let event = toRepositoryCreated repo
            return (repo, [event])
        | i when i.Signature.IsNone ->
            let repo = Unsigned path
            let event = toRepositoryCreated repo
            return (repo, [event])
        | _ ->
            let repo = Damaged path
            let event = toRepositoryCreated repo
            return (repo, [event])
    }

let tryBump (repo: Repository) (nextVersion: CalendarVersion) =
    result {
        match repo with        
        | Ready (dir, head, signature, currentVersion) ->
            let sameVersion = currentVersion
                            |> Option.map (fun v -> v = nextVersion)
                            |> Option.defaultValue false
            if sameVersion
            then
                return! CurrentRepository |> Error
            else
                let repo = Ready (dir, head, signature, Some nextVersion)
                let event = RepositoryBumped {
                    Version = nextVersion
                    Signature = signature
                    State = toState repo
                }
                return (event, event)
        | Dirty _    -> return! DirtyRepository    |> Error
        | Unborn _   -> return! UnbornRepository   |> Error
        | Unsigned _ -> return! UnsignedRepository |> Error
        | Damaged _  -> return! DamagedRepository  |> Error
    }