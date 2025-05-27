module Calaf.Application.Bump

open FsToolkit.ErrorHandling

open Calaf.Domain

let run (path: string) (context: BumpContext) (settings: BumpSettings) =
    result {
            let timeStamp = context.Clock.now()            
            let! monthStamp = DateSteward.tryCreate timeStamp.DateTime |> Result.mapError CalafError.Domain
            
            let (DotNetXmlFilePattern searchPatternStr) = settings.ProjectsSearchPattern
            let! dir = context.FileSystem.tryReadDirectory path searchPatternStr
            
            let (TagQuantity tagCount) = settings.TagsToLoad
            let! repo = context.Git.tryRead path tagCount Version.versionPrefixes timeStamp
            
            let! workspace, createEvents = Workspace.tryCapture (dir, repo) |> Result.mapError CalafError.Domain           
            
            let! bumpedWorkspace, bumpEvents = Workspace.tryBump workspace monthStamp |> Result.mapError CalafError.Domain
            
            do! bumpedWorkspace.Suite
                |> Suite.tryProfile
                |> Seq.traverseResultM (fun p -> context.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore
                            
            return bumpedWorkspace
        }