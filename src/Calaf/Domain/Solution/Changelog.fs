module internal Calaf.Domain.Changelog

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let tryCapture (fileInfo: FileInfo) : Changelog =
    { Metadata = {
        Name = fileInfo.Name
        Extension = fileInfo.Extension
        Directory = fileInfo.Directory
        AbsolutePath = fileInfo.AbsolutePath }
      FileExists = fileInfo.Exists }
    
let snapshot
    (workspace: Workspace)
    (changeset: Changeset)=
    match workspace.Solution with
    | Standard { Changelog = changelog } ->
        let content = Changeset.toString changeset workspace.Version
        { AbsolutePath = changelog.Metadata.AbsolutePath
          ExistsInFileSystem = changelog.FileExists
          ChangesetContent = content }