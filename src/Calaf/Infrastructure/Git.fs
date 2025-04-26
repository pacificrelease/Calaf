namespace Calaf

open System.IO
open LibGit2Sharp

open Calaf.Contracts
open Calaf.Domain.Errors

module internal Git =  
            
    let private createGitRepository (repo: Repository) =
        let currentBranch = repo.Head.FriendlyName
        let dirty = repo.RetrieveStatus().IsDirty
        let headDetached = repo.Info.IsHeadDetached
        { Directory = repo.Info.WorkingDirectory
          Dirty = dirty
          HeadDetached = headDetached          
          CurrentBranch = currentBranch }            
    
    let TryReadRepository (path: DirectoryInfo) =
        try
            if Repository.IsValid(path.FullName)
            then
                use repo = new Repository(path.FullName)
                repo
                |> createGitRepository
                |> Ok
            else
               path.FullName
                |> NoGitRepository 
                |> Git
                |> Error
        with exn ->                     
            exn
            |> RepositoryAccessError
            |> Git
            |> Error