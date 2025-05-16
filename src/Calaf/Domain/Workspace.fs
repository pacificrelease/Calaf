module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let tryCreate (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) =
    result {
        let suite, events =
            directory.Projects
            |> Array.map Project.tryCreate
            |> Array.choose id
            |> Suite.create
            
        let! repoResult =
            repoInfo
            |> Option.traverseResult Repository.tryCreate            
        let events = match repoResult with | Some (_, e) -> events @ e | None -> events
        
        return { Directory  = directory.Directory
                 Repository = repoResult  |> Option.map fst 
                 Suite      = suite }
    }