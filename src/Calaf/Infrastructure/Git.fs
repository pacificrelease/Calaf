namespace Calaf.Infrastructure

open LibGit2Sharp

open Calaf.Infrastructure

module internal Git =
    let private validTag (tag: Tag) =
        not (System.String.IsNullOrWhiteSpace tag.FriendlyName)
        
    let private extractTags (repo: Repository) (maxTagsToRead: int) =
        repo.Tags        
        |> Seq.filter validTag
        |> Seq.truncate maxTagsToRead
        |> Seq.toArray
        
    let private extractRepositoryContext (repo: Repository)=
        { Head = repo.Head
          Status = RepositoryExtensions.RetrieveStatus repo
          WorkingDirectory = repo.Info.WorkingDirectory
          IsHeadUnborn = repo.Info.IsHeadUnborn
          IsHeadDetached = repo.Info.IsHeadDetached }
        
    let tryReadRepository (path: string) (maxTagsToRead: int) (timeStamp: System.DateTimeOffset) =
        try
            if Repository.IsValid(path)
            then
                use repo = new Repository(path)
                let ctx = extractRepositoryContext repo
                let tags = extractTags repo maxTagsToRead
                let signature = repo.Config.BuildSignature timeStamp
                let gitRepoInfo = Mappings.toGitRepositoryInfo ctx tags signature
                gitRepoInfo |> Some |> Ok
            else
                None |> Ok
        with exn ->                     
            exn |> RepoAccessFailed |> Git |> Error
            
    let tryCommitRepository
        (path: string)
        (message: string)
        (author: Signature)
        (committer: Signature)=
        try
            if Repository.IsValid(path)
            then
                use repo = new Repository(path)
                
                
                let author = Signature(author.Name, author.Email, author.When)
                let committer = Signature(committer.Name, committer.Email, committer.When)
                
                // Create a commit with the specified message and author/committer signatures
                let commit = repo.Commit(message, author, committer)
                commit |> Some |> Ok
            else
                RepoNotInitialized |> Git |> Error
                
        with exn ->
            exn |> RepoAccessFailed |> Git |> Error