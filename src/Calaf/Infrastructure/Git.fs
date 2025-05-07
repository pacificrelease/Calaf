namespace Calaf.Infrastructure

open System.IO
open LibGit2Sharp

open Calaf.Contracts

module internal Git =
    let rec private tryMapCommit (target: obj) : Commit option =
        match target with
        | :? Commit as c         -> Some c
        | :? TagAnnotation as ta -> tryMapCommit ta.Target
        | _                      -> None
    
    let private createGitCommitInfo (commit: Commit) : GitCommitInfo =
        { Hash = commit.Sha; Message = commit.MessageShort; When = commit.Committer.When }
    
    let private tryExtractCommitInfo (tag: Tag) : GitCommitInfo option =            
        tag.Target |> tryMapCommit |> Option.map createGitCommitInfo        
        
    let private tryGetTagInfo (tag: Tag) : GitTagInfo option =        
        if not (System.String.IsNullOrWhiteSpace tag.FriendlyName) then            
            Some { Name = tag.FriendlyName; Commit = tryExtractCommitInfo tag }
        else None
        
    let private readTags (repo: Repository) (maxTagsCount: int) : GitTagInfo seq =
        repo.Tags        
        |> Seq.map tryGetTagInfo
        |> Seq.choose id
        |> Seq.map (fun t -> t, t.Commit |> Option.map _.When)
        |> Seq.sortByDescending snd
        |> Seq.truncate maxTagsCount
        |> Seq.map fst
        
    let private createGitRepository (repo: Repository) (tagsQty: int)=
        let info   = repo.Info
        let status = repo.RetrieveStatus()
        let branch =
            if info.IsHeadUnborn || info.IsHeadDetached then None
            else Some repo.Head.FriendlyName
        let commit =
            if info.IsHeadUnborn then None
            else repo.Head.Tip |> createGitCommitInfo |> Some
        { Directory     = info.WorkingDirectory
          Damaged       = not info.IsHeadUnborn && isNull repo.Head.Tip 
          Unborn        = info.IsHeadUnborn
          Detached      = info.IsHeadDetached
          Dirty         = status.IsDirty          
          CurrentBranch = branch
          CurrentCommit = commit
          Tags          = readTags repo tagsQty |> Seq.toArray }
    
    let tryReadRepository (path: DirectoryInfo) (tagsQty: int) =
        try
            if Repository.IsValid(path.FullName)
            then
                use repo = new Repository(path.FullName)
                createGitRepository repo tagsQty |> Some |> Ok
            else
                None |> Ok
        with exn ->                     
            exn |> RepoAccessFailed |> Git |> Error