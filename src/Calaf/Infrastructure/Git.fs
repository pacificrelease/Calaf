namespace Calaf

open System.IO
open LibGit2Sharp

open Calaf.Contracts
open Calaf.Domain.Errors

module internal Git =
    let private tryExtractTagTimestamp (tag: Tag) =
        match tag.Target with
        | :? TagAnnotation as ta -> Some ta.Tagger.When
        | :? Commit as c         -> Some c.Committer.When
        | _                      -> None
        
    let private toTag (tag: Tag) =
        { Name = tag.FriendlyName
          When = tryExtractTagTimestamp tag }
        
    let private readTags (repo: Repository) =
        repo.Tags
        |> Seq.map toTag
        |> Seq.sortByDescending(_.When)
        |> Seq.toArray
        
    let private createGitRepository (repo: Repository) =
        { Directory = repo.Info.WorkingDirectory
          Dirty = repo.RetrieveStatus().IsDirty
          HeadDetached = repo.Info.IsHeadDetached          
          CurrentBranch = repo.Head.FriendlyName
          Tags = repo |> readTags }
    
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