namespace Calaf

open System.IO
open LibGit2Sharp

open Calaf.Contracts
open Calaf.Domain.Errors

module internal Git =
    let private toTag (tag: Tag) =
        match tag.Target with
        | :? TagAnnotation as tagAnnotation ->
            { Name = tag.FriendlyName; When = Some tagAnnotation.Tagger.When }
        | :? Commit as commit ->
            { Name = tag.FriendlyName; When = Some commit.Committer.When }
        | _ ->
            { Name = tag.FriendlyName; When = None }
            
    let private createGitRepository (repo: Repository) =
        { Directory = repo.Info.WorkingDirectory
          Dirty = repo.RetrieveStatus().IsDirty
          HeadDetached = repo.Info.IsHeadDetached          
          CurrentBranch = repo.Head.FriendlyName
          Tags = repo.Tags |> Seq.map toTag |> Seq.toArray }
    
    let tryReadRepository (path: DirectoryInfo) =
        try
            if Repository.IsValid(path.FullName)
            then
                use repo = new Repository(path.FullName)
                repo |> createGitRepository |> Ok
            else
               path.FullName |> NoGitRepository |> Git |> Error
        with exn ->                     
            exn |> RepositoryAccessError |> Git |> Error