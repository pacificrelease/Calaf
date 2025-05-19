namespace Calaf.Infrastructure

type internal RepositoryContext = {
    Head: LibGit2Sharp.Branch
    Status: LibGit2Sharp.RepositoryStatus
    WorkingDirectory: string
    IsHeadUnborn: bool
    IsHeadDetached: bool    
}

module internal Mappings =
    
    open Calaf.Contracts
    
    // Git
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
        (repoCtx: RepositoryContext)
        (tags: LibGit2Sharp.Tag list)
        (signature: LibGit2Sharp.Signature) =    
        let branch =
            if repoCtx.IsHeadUnborn || repoCtx.IsHeadDetached
            then None
            else Some repoCtx.Head.FriendlyName
        let commit =
            if repoCtx.IsHeadUnborn
            then None
            else repoCtx.Head.Tip |> toGitCommitInfo |> Some
        let signature =
            if isNull signature
            then None
            else signature |> toGitSignature |> Some
        { Directory     = repoCtx.WorkingDirectory
          Damaged       = not repoCtx.IsHeadUnborn && isNull repoCtx.Head.Tip 
          Unborn        = repoCtx.IsHeadUnborn
          Detached      = repoCtx.IsHeadDetached
          Dirty         = repoCtx.Status.IsDirty
          CurrentBranch = branch
          CurrentCommit = commit
          Signature     = signature      
          Tags          = tags |> List.map toGitTagInfo }
         
    // FileSystem
    let toProjectXmlFileInfo (file: System.IO.FileInfo) (xml: System.Xml.Linq.XElement) : ProjectXmlFileInfo =
        { Name = file.Name
          Directory = file.DirectoryName
          Extension = file.Extension
          AbsolutePath = file.FullName
          Content = xml }
        
    let toWorkspaceDirectoryInfo (directoryInfo: System.IO.DirectoryInfo) (projectsInfos: (System.IO.FileInfo * System.Xml.Linq.XElement) seq) : DirectoryInfo =
        let projectsInfos = 
            projectsInfos
            |> Seq.map (fun (fileInfo, xml) -> toProjectXmlFileInfo fileInfo xml)
            |> Seq.toList
        { Directory = directoryInfo.FullName
          Projects = projectsInfos }
            
