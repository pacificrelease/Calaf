namespace Calaf

open System.IO
open LibGit2Sharp

open Calaf.Contracts
open Calaf.Domain.Errors

module internal Git =
    type TagsQuantity = byte
    [<Literal>]
    let private tenTags = 10uy
    
    let private tryGetCommitInfo (tag: Tag) =
        let rec findCommit (target: obj) =
            match target with
            | :? Commit as c         -> Some c
            | :? TagAnnotation as ta -> findCommit ta.Target
            | _                      -> None
        findCommit tag.Target
        |> Option.map (fun c ->
            { Hash = c.Sha
              Message = c.MessageShort
              When = c.Committer.When })
        
    let private tryGetTagInfo (tag: Tag) =        
        if not (System.String.IsNullOrWhiteSpace tag.FriendlyName) then            
            Some { Name = tag.FriendlyName; Commit = tryGetCommitInfo tag }
        else None
        
    let private readTags (repo: Repository) (qty: TagsQuantity) =
        repo.Tags
        |> Seq.truncate (int qty)
        |> Seq.map tryGetTagInfo
        |> Seq.choose id
        |> Seq.map (fun t -> t, t.Commit |> Option.map _.When)
        |> Seq.sortByDescending snd
        |> Seq.map fst
        |> Seq.toArray
        
    let private createGitRepository (repo: Repository) =
        { Directory = repo.Info.WorkingDirectory
          Dirty = repo.RetrieveStatus().IsDirty
          HeadDetached = repo.Info.IsHeadDetached          
          CurrentBranch = repo.Head.FriendlyName
          Tags = readTags repo tenTags }
    
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