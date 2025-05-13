module internal Calaf.Domain.Repository

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

let tryCreate (repoInfo: GitRepositoryInfo) =
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
        match repoInfo with
        | { Damaged = true } ->
            return Damaged repoInfo.Directory            
        | i when i.Unborn || i.CurrentCommit.IsNone ->
            return Unborn repoInfo.Directory            
        | i when i.Dirty &&
                 i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            return! tryCreate Repository.Dirty i.Signature.Value i.CurrentCommit.Value i.CurrentBranch            
        | i when i.CurrentCommit.IsSome &&
                 i.Signature.IsSome ->
            return! tryCreate Repository.Ready i.Signature.Value i.CurrentCommit.Value i.CurrentBranch            
        | i when i.Signature.IsNone ->
            return Unsigned repoInfo.Directory            
        | _ ->
            return Damaged repoInfo.Directory
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
                let event = ReadyRepositoryBumped {
                    Version = nextVersion
                    Signature = signature
                }
                return Ready (dir, head, signature, Some nextVersion), event                
        | Dirty _    -> return! DirtyRepository    |> Error
        | Unborn _   -> return! UnbornRepository   |> Error
        | Unsigned _ -> return! UnsignedRepository |> Error
        | Damaged _  -> return! DamagedRepository  |> Error
    }