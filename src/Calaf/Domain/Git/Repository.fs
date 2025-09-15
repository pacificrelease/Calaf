module internal Calaf.Domain.Repository

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes
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
            | Dirty (_, { Version = Some { Version = calendarVersion } })
            | Ready (_, { Version = Some { Version = calendarVersion } }) -> Some calendarVersion
            | _ -> None
        { Version = version; State = state }
        |> RepositoryEvent.StateCaptured
        |> DomainEvent.Repository
        
    let toRepositoryReleased repo version signature =
        let state = toState repo        
        { Version = version; Signature = signature; State = state}
        |> RepositoryEvent.ReleaseProvided
        |> DomainEvent.Repository
        
let private createRepositoryCalendarVersion calendarVersion =
    { TagName = Version.toTagName calendarVersion
      Version = CalVer calendarVersion
      CommitMessage =
          Version.toCommitText calendarVersion
          |> CommitMessage.create
          |> Some}

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
                let version =
                   repoInfo.Tags
                   |> Seq.map Tag.create
                   |> Tag.chooseCalendarVersions        
                   |> Version.tryMax2
                return ctor (repoInfo.Directory, { Head = head; Signature = signature; Version = version })
            }
        let! path = tryValidatePath repoInfo.Directory
        match repoInfo with        
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
    | Ready (_, { Version = Some { Version = (CalVer version) } }) -> Some version
    | Dirty (_, { Version = Some { Version = (CalVer version) } }) -> Some version
    | _ -> None
    
let trySnapshot
    (repo: Repository)
    (pendingFilesPaths: string list)    =    
    match repo with        
    | Ready (dir, { Signature = signature; Version = Some { Version = (CalVer currentVersion) } })        
    | Dirty (dir, { Signature = signature; Version = Some { Version = (CalVer currentVersion) } }) ->
        //TODO: Remove all
        let tagName = Version.toTagName currentVersion
        let commitText = Version.toCommitText currentVersion
        Some { Directory = dir
               PendingFilesPaths = pendingFilesPaths
               Signature = signature
               TagName = tagName
               CommitText = commitText }
    | _ -> None    
    
let tryRelease (repo: Repository) (nextVersion: CalendarVersion) =
    let release (ctor, dir, metadata: RepositoryMetadata) =
        let sameVersion =
            metadata.Version
            |> Option.map (fun x -> x.Version = (CalVer nextVersion))
            |> Option.exists id
        if sameVersion
        then
            Error RepositoryAlreadyCurrent
        else            
            let repo = ctor (dir, { metadata with Version = createRepositoryCalendarVersion nextVersion |> Some })
            let event = Events.toRepositoryReleased repo nextVersion metadata.Signature
            Ok (repo, [event])
    
    match repo with        
    | Ready (dir, metadata) ->
        release (Repository.Ready, dir, metadata)
    | Dirty (dir, metadata) ->
        release (Repository.Dirty, dir, metadata)
    | Unborn   _ -> RepositoryHeadUnborn |> Error
    | Unsigned _ -> RepositoryUnsigned   |> Error
    | Damaged  _ -> RepositoryCorrupted  |> Error