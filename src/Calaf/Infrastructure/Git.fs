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
       
     // string -> string -> LibGit2Sharp.Signature -> Result<Commit, InfrastructureError>
    let tryCommit
    // TODO: Replace to DirectoryInfo?
        (directory: string)
        (message: string)
        (signature: Signature)=
        try
            if Repository.IsValid(directory)
            then
                use repo = new Repository(directory)
                let commit = repo.Commit(message, signature, signature)
                commit |> Ok
            else
                RepoNotInitialized |> Git |> Error
        with exn ->
            exn |> RepoAccessFailed |> Git |> Error
    
    // string -> string -> Commit -> Result<Tag, InfrastructureError>
    let tryTag
    // TODO: Replace to DirectoryInfo?
        (directory: string)
        (tagName: string)
        (commit: Commit) =
        try
           if Repository.IsValid(directory)
           then
            use repo = new Repository(directory)
            let tag = repo.Tags.Add(tagName, commit)
            tag |> Ok
           else
               RepoNotInitialized |> Git |> Error           
        with exn ->
            exn |> RepoAccessFailed |> Git |> Error