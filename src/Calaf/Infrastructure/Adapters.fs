namespace Calaf.Infrastructure

open Calaf.Contracts
open Calaf.Application

module internal Repository =    
    open LibGit2Sharp   
    
    let read
        (directory: string)
        (maxTagsToRead: int)
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

module internal File =
    let load
        (absolutePath: string) : Result<System.Xml.Linq.XElement, InfrastructureError> =
        try
            let settings = System.Xml.XmlReaderSettings()
            settings.DtdProcessing <- System.Xml.DtdProcessing.Prohibit
            settings.XmlResolver <- null
            use reader = System.Xml.XmlReader.Create(absolutePath, settings)
            let xml = System.Xml.Linq.XElement.Load(reader)            
            xml |> Ok
        with
        | :? System.UnauthorizedAccessException as exn ->
            (absolutePath, exn :> System.Exception)
            |> FileAccessDenied
            |> FileSystem
            |> Error
        | exn ->
            (absolutePath, exn)
            |> XmlLoadFailed
            |> FileSystem
            |> Error
    
    let save
        (absolutePath: string)
        (content: System.Xml.Linq.XElement) : Result<System.Xml.Linq.XElement, InfrastructureError> =
        try        
            let options = System.Xml.Linq.SaveOptions.None
            content.Save(absolutePath, options)
            content |> Ok
        with
        | :? System.UnauthorizedAccessException as exn ->
            (absolutePath, exn :> System.Exception)
            |> FileAccessDenied
            |> FileSystem
            |> Error
        | exn ->
            (absolutePath, exn)
            |> XmlSaveFailed
            |> FileSystem
            |> Error
    
module internal Directory =
    open FsToolkit.ErrorHandling
    open System.IO
    
    open Calaf.Extensions.InternalExtensions

    let private find
        (directory: DirectoryInfo)
        (pattern: string) =
        try
            directory.GetFiles(pattern, SearchOption.AllDirectories)
            |> Array.toList
            |> Ok
        with exn ->
            exn
            |> FilesScanFailed
            |> FileSystem
            |> Error        

    let private info
        (path: string) =
        try
            let path = DirectoryInfo path
            if path.Exists
            then path |> Ok
            else DirectoryDoesNotExist
                 |> FileSystem
                 |> Error
        with exn ->
            exn
            |> DirectoryAccessDenied
            |> FileSystem
            |> Error
            
    let list
        (path: string)
        (pattern: string) =
        result {
            let! dirInfo = info path
            let! files = find dirInfo pattern
            let projects, _ =
                files
               |> List.map (fun fileInfo -> File.load fileInfo.FullName
                                            |> Result.map (fun xml ->  fileInfo, xml))
               |> Result.partition                                   
            return Mappings.toWorkspaceDirectoryInfo dirInfo projects
        }

type Git() =
    interface IGit with
        member _.tryRead directory maxTagsToRead tagsPrefixesToFilter timeStamp =
            Repository.read directory maxTagsToRead tagsPrefixesToFilter timeStamp
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply directory commitMessage tagName signature =
            Repository.apply directory commitMessage tagName signature
            |> Result.mapError CalafError.Infrastructure

type FileSystem() =
    interface IFileSystem with
        member _.tryReadDirectory directory pattern =
            Directory.list directory pattern
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryReadXml absolutePath =
            File.load absolutePath
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryWriteXml absolutePath content =
            File.save absolutePath content
            |> Result.mapError CalafError.Infrastructure
            |> Result.map ignore
            
type Clock() =
    interface IClock with
        member _.now() =
            System.DateTimeOffset.UtcNow