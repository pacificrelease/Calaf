module Calaf.Application.Bump

open FsToolkit.ErrorHandling

open Calaf.Contracts
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
            
            let! workspace', bumpEvents = Workspace.tryBump workspace monthStamp |> Result.mapError CalafError.Domain
            
            let profile = Workspace.profile workspace'
            do! profile.Projects
                |> List.traverseResultM (fun p -> context.FileSystem.tryWriteXml (p.AbsolutePath, p.Content))
                |> Result.map ignore                
            do! profile.Repository
                |> Option.map (fun p ->
                    let signature = { Name = p.Signature.Name; Email = p.Signature.Email; When = p.Signature.When }
                    context.Git.tryApply (p.Directory, p.Files) p.CommitMessage p.TagName signature
                    |> Result.map ignore
                    |> Result.mapError id)
                |> Option.defaultValue (Ok ())
                            
            return workspace'
        }