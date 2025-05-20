namespace Calaf.Infrastructure

open LibGit2Sharp

open Calaf.Contracts
open Calaf.Infrastructure

module internal Git =
    let private validTag (tag: Tag) =
        not (System.String.IsNullOrWhiteSpace tag.FriendlyName)
        
    let private extractTags (repo: Repository) (maxTagsToRead: int) =
        repo.Tags        
        |> Seq.filter validTag
        |> Seq.truncate maxTagsToRead
        |> Seq.toList
        
    let private extractRepositoryContext (repo: Repository)=
        { Head = repo.Head
          Status = RepositoryExtensions.RetrieveStatus repo
          WorkingDirectory = repo.Info.WorkingDirectory
          IsHeadUnborn = repo.Info.IsHeadUnborn
          IsHeadDetached = repo.Info.IsHeadDetached }
        
    // string -> int -> System.DateTimeOffset -> Result<GitRepositoryInfo, InfrastructureError> 
    let tryReadRepository
    // TODO: Replace to DirectoryInfo?
        (directory: string)
        (maxTagsToRead: int)
        (timeStamp: System.DateTimeOffset) =
        try
            if Repository.IsValid(directory)
            then
                use repo = new Repository(directory)
                let ctx = extractRepositoryContext repo
                let tags = extractTags repo maxTagsToRead
                let signature = repo.Config.BuildSignature timeStamp
                let gitRepoInfo = Mappings.toGitRepositoryInfo ctx tags signature
                gitRepoInfo |> Some |> Ok
            else
                None |> Ok
        with exn ->                     
            exn |> RepoAccessFailed |> Git |> Error
            
    let tryApply
        // TODO: Combine arguments to command
        (directory: string)
        // Affect only changed projects in the future
        (commitMessage: string)
        (tagName: string)
        (signature: GitSignatureInfo) : Result<unit, InfrastructureError> =
        let totalPaths = "*"
        let signature =
            Signature(signature.Name, signature.Email, signature.When)
            
        let unstage repo =
            Commands.Unstage(repo, totalPaths)
            repo
            
        let stage repo =
            Commands.Stage(repo, totalPaths)
            repo
        
        let commit (repo: Repository) =
            repo.Commit(commitMessage, signature, signature) |> ignore
            repo
            
        let tag (repo: Repository) (commit: Commit) =            
            repo.Tags.Add(tagName, commit) |> ignore
            repo
        
        try
            if Repository.IsValid(directory)
            then
                use repo = new Repository(directory)
                repo
                |> unstage
                |> stage
                |> commit
                |> tag
                |> ignore
                |> Ok
            else
                RepoNotInitialized |> Git |> Error 
        with exn ->
            exn |> RepoAccessFailed |> Git |> Error