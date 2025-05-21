namespace Calaf.Infrastructure

open Calaf.Contracts
open Calaf.Application

module internal GitRepository =    
    open LibGit2Sharp   
    
    let read
        (directory: string)
        (maxTagsToRead: int)
        (timeStamp: System.DateTimeOffset) =        
        let validTag (tag: Tag) =
            not (System.String.IsNullOrWhiteSpace tag.FriendlyName)
            
        let extractTags (repo: Repository) (maxTagsToRead: int) =
            repo.Tags        
            |> Seq.filter validTag
            |> Seq.truncate maxTagsToRead
            |> Seq.toList
            
        let extractRepositoryContext (repo: Repository) =
            { Head = repo.Head
              Status = RepositoryExtensions.RetrieveStatus repo
              WorkingDirectory = repo.Info.WorkingDirectory
              IsHeadUnborn = repo.Info.IsHeadUnborn
              IsHeadDetached = repo.Info.IsHeadDetached }
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
        
    let apply
        // TODO: Combine arguments to command
        // TODO: Accept as an argument only changed projects instead of all
        (directory: string)        
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

type Git() =
    interface IGit with
        member _.tryRead directory maxTagsToRead timeStamp =
            GitRepository.read directory maxTagsToRead timeStamp |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply directory commitMessage tagName signature =
            GitRepository.apply directory commitMessage tagName signature |> Result.mapError CalafError.Infrastructure



type FileSystem() =
    interface IFileSystem with
        member this.tryReadDirectory directory pattern =
            failwith "todo"
            
        member this.tryReadXml absolutePath =
            failwith "todo"
            
        member this.tryWriteXml absolutePath content =
            failwith "todo"
            
type Clock() =
    interface IClock with
        member _.now() =
            System.DateTimeOffset.UtcNow