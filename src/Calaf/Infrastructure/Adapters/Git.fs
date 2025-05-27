namespace Calaf.Infrastructure

open Calaf.Contracts
open Calaf.Application

type internal GitRepositoryContext = {
    Head: LibGit2Sharp.Branch
    Status: LibGit2Sharp.RepositoryStatus
    WorkingDirectory: string
    IsHeadUnborn: bool
    IsHeadDetached: bool    
}

module internal GitGateway =
    let rec private extractCommit (target: obj) : LibGit2Sharp.Commit option =
        match target with
        | :? LibGit2Sharp.Commit as c -> Some c
        | :? LibGit2Sharp.TagAnnotation as ta -> extractCommit ta.Target
        | _ -> None
        
    let toGitSignature (signature: LibGit2Sharp.Signature) =    
        { Name = signature.Name
          Email = signature.Email
          When = signature.When }
        
    let toGitCommitInfo (commit: LibGit2Sharp.Commit) =
         { Hash = commit.Sha
           Message = commit.MessageShort
           When = commit.Committer.When }
         
    let toGitTagInfo (tag: LibGit2Sharp.Tag) =
        { Name = tag.FriendlyName
          Commit = tag.Target |> extractCommit |> Option.map toGitCommitInfo }
        
    let toGitRepositoryInfo
        ctx
        (tags: LibGit2Sharp.Tag list)
        (signature: LibGit2Sharp.Signature) =    
        let branch =
            if ctx.IsHeadUnborn || ctx.IsHeadDetached
            then None
            else Some ctx.Head.FriendlyName
        let commit =
            if ctx.IsHeadUnborn
            then None
            else ctx.Head.Tip |> toGitCommitInfo |> Some
        let signature =
            if isNull signature
            then None
            else signature |> toGitSignature |> Some
        { Directory     = ctx.WorkingDirectory
          Damaged       = not ctx.IsHeadUnborn && isNull ctx.Head.Tip 
          Unborn        = ctx.IsHeadUnborn
          Detached      = ctx.IsHeadDetached
          Dirty         = ctx.Status.IsDirty
          CurrentBranch = branch
          CurrentCommit = commit
          Signature     = signature      
          Tags          = tags |> List.map toGitTagInfo }
    
module internal GitRepository =    
    open LibGit2Sharp
    
    let read
        (directory: string)
        (maxTagsToRead: byte)
        (tagsPrefixesToFilter: string list)
        (timeStamp: System.DateTimeOffset) =        
        let validTag (tag: Tag) =
            tagsPrefixesToFilter
            |> List.exists (fun prefix ->
                not (System.String.IsNullOrEmpty(tag.FriendlyName)) &&
                tag.FriendlyName.Contains prefix)
            
        let extractTags (repo: Repository) (maxTagsToRead: int) =
            repo.Tags        
            |> Seq.filter validTag
            |> Seq.truncate maxTagsToRead
            |> Seq.toList
            
        let extractContext (repo: Repository) =
            { Head = repo.Head
              Status = RepositoryExtensions.RetrieveStatus repo
              WorkingDirectory = repo.Info.WorkingDirectory
              IsHeadUnborn = repo.Info.IsHeadUnborn
              IsHeadDetached = repo.Info.IsHeadDetached }
               
        try
            if Repository.IsValid(directory)
            then
                use repo = new Repository(directory)
                let ctx = extractContext repo
                let tags = maxTagsToRead |> int |> extractTags repo 
                let signature = repo.Config.BuildSignature timeStamp
                let gitRepoInfo = GitGateway.toGitRepositoryInfo ctx tags signature
                gitRepoInfo |> Some |> Ok
            else
                None |> Ok
        with exn ->                     
            exn |> RepoAccessFailed |> Git |> Error
        
    let apply
        // TODO: Combine arguments to command
        // TODO: Accept as an argument only changed projects instead of all
        (directory: string)
        (files: string list)
        (commitMessage: string)
        (tagName: string)
        (signature: GitSignatureInfo) : Result<unit, InfrastructureError> =            
        let all = "*"
        let signature =
            Signature(signature.Name, signature.Email, signature.When)
            
        let unstage repo =
            Commands.Unstage(repo, all)
            repo
            
        let stage (files: string list) repo =
            Commands.Stage(repo, files)
            repo
        
        let commit (repo: Repository) =
            let commit = repo.Commit(commitMessage, signature, signature)
            (repo, commit)
            
        let tag (repo: Repository, commit: Commit) =
            repo.Tags.Add(tagName, commit) |> ignore
            repo
        
        try
            if Repository.IsValid(directory)
            then
                use repo = new Repository(directory)
                repo
                |> unstage
                |> stage files
                |> commit
                |> tag
                |> ignore
                |> Ok
            else
                RepoNotInitialized |> Git |> Error 
        with exn ->
            exn |> RepoAccessFailed |> Git |> Error

type Git() =
    interface IGit with
        member _.tryRead directory maxTagsToRead tagsPrefixesToFilter timeStamp =
            GitRepository.read directory maxTagsToRead tagsPrefixesToFilter timeStamp
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply (directory, files) commitMessage tagName signature =
            GitRepository.apply directory files commitMessage tagName signature
            |> Result.mapError CalafError.Infrastructure