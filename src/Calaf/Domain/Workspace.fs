module internal Calaf.Domain.Workspace

open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let tryCreate (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) =
    result {
        let suite =
            directory.Projects
            |> Array.map Project.tryCreate
            |> Array.choose id
            |> Suite.create
            
        let! repoResult = repoInfo |> Option.traverseResult Repository.tryCreate
        return { Directory  = directory.Directory
                 Repository = repoResult |> Option.map fst 
                 Suite      = suite }
    }