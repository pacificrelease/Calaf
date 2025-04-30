namespace Calaf.Infrastructure

open System.IO
open LibGit2Sharp

open Calaf.Contracts
open Calaf.Domain.Errors

module internal Git =
    [<Literal>]
    let private hundredTags = 100
    
    let rec tryMapCommit (target: obj) =
        match target with
        | :? Commit as c         -> Some c
        | :? TagAnnotation as ta -> tryMapCommit ta.Target
        | _                      -> None
    
    let private createGitCommitInfo (commit: Commit) =
        { Hash = commit.Sha; Message = commit.MessageShort; When = commit.Committer.When }
    
    let private tryExtractCommitInfo (tag: Tag) =            
        tag.Target |> tryMapCommit |> Option.map createGitCommitInfo        
        
    let private tryGetTagInfo (tag: Tag) =        
        if not (System.String.IsNullOrWhiteSpace tag.FriendlyName) then            
            Some { Name = tag.FriendlyName; Commit = tryExtractCommitInfo tag }
        else None
        
    let private readTags (repo: Repository) maxTagsCount=
        repo.Tags        
        |> Seq.map tryGetTagInfo
        |> Seq.choose id
        |> Seq.map (fun t -> t, t.Commit |> Option.map _.When)
        |> Seq.sortByDescending snd
        |> Seq.truncate maxTagsCount
        |> Seq.map fst
        
    let private createGitRepository (repo: Repository) =
        let info   = repo.Info
        let status = repo.RetrieveStatus()
        let branch =
            if info.IsHeadUnborn || info.IsHeadDetached then None
            else Some repo.Head.FriendlyName
        let commit =
            if info.IsHeadUnborn then None
            else repo.Head.Tip |> createGitCommitInfo |> Some
        { Directory     = info.WorkingDirectory
          Damaged       = isNull repo.Head.Tip
          Unborn        = info.IsHeadUnborn
          Detached      = info.IsHeadDetached
          Dirty         = status.IsDirty          
          CurrentBranch = branch
          CurrentCommit = commit
          Tags          = readTags repo hundredTags |> Seq.toArray }
    
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