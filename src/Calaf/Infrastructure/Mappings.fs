module internal Calaf.Infrastructure.Mappings

open Calaf.Contracts

// Git
let rec private extractCommit (target: obj) : LibGit2Sharp.Commit option =
    match target with
    | :? LibGit2Sharp.Commit as c -> Some c
    | :? LibGit2Sharp.TagAnnotation as ta -> extractCommit ta.Target
    | _ -> None
    
let toGitCommitInfo (commit: LibGit2Sharp.Commit) =
     { Hash = commit.Sha
       Message = commit.MessageShort
       When = commit.Committer.When }
     
let toGitTagInfo (tag: LibGit2Sharp.Tag) =
    { Name = tag.FriendlyName
      Commit = tag.Target |> extractCommit |> Option.map toGitCommitInfo }
    
let private validTag (tag: LibGit2Sharp.Tag) =
    not (System.String.IsNullOrWhiteSpace tag.FriendlyName)
    
let private readTags (repo: LibGit2Sharp.Repository) (maxTagsToRead: int) : GitTagInfo seq =
    repo.Tags        
    |> Seq.filter validTag
    |> Seq.map toGitTagInfo
    |> Seq.map (fun t -> t, t.Commit |> Option.map _.When)
    |> Seq.sortByDescending snd
    |> Seq.truncate maxTagsToRead
    |> Seq.map fst
    
let toGitRepositoryInfo (repo: LibGit2Sharp.Repository) (maxTagsToRead: int) =
    let info   = repo.Info
    let status = LibGit2Sharp.RepositoryExtensions.RetrieveStatus repo
    let branch =
        if info.IsHeadUnborn || info.IsHeadDetached then None
        else Some repo.Head.FriendlyName
    let commit =
        if info.IsHeadUnborn then None
        else repo.Head.Tip |> toGitCommitInfo |> Some
    { Directory     = info.WorkingDirectory
      Damaged       = not info.IsHeadUnborn && isNull repo.Head.Tip 
      Unborn        = info.IsHeadUnborn
      Detached      = info.IsHeadDetached
      Dirty         = status.IsDirty
      CurrentBranch = branch
      CurrentCommit = commit
      Tags          = readTags repo maxTagsToRead |> Seq.toArray }
     
// FileSystem
let toProjectFileInfo (file: System.IO.FileInfo) (xml: System.Xml.Linq.XElement) : ProjectInfo =
    { Name = file.Name
      Directory = file.DirectoryName
      Extension = file.Extension
      AbsolutePath = file.FullName
      Payload = xml }
    
let toWorkspaceDirectoryInfo (directoryInfo: System.IO.DirectoryInfo) (projectsInfos: (System.IO.FileInfo * System.Xml.Linq.XElement) seq) : DirectoryInfo =
    let projectsInfos = 
        projectsInfos
        |> Seq.map (fun (fileInfo, xml) -> toProjectFileInfo fileInfo xml)
        |> Seq.toArray
    { Directory = directoryInfo.FullName
      Projects = projectsInfos }
        
