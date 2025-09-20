module internal Calaf.Domain.ChangeLog

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
    (releaseNotes: ReleaseNotes)=
    match workspace.Solution with
    | Standard { Changelog = changelog } ->
        let releaseNotesContent = ReleaseNotes.toString releaseNotes
        { AbsolutePath = changelog.Metadata.AbsolutePath
          ExistsInFileSystem = changelog.FileExists
          ReleaseNotesContent = releaseNotesContent }