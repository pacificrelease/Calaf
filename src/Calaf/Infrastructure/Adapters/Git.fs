namespace Calaf.Infrastructure

open System
open System.Diagnostics
open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Application

module internal GitWrapper =    
    let private runGit
        (directory: string)
        (arguments: string)=
        try
            let successExitCode = 0
            let gitCommand = "git"
            
            let startInfo = ProcessStartInfo(gitCommand, arguments)
            startInfo.RedirectStandardOutput <- true
            startInfo.RedirectStandardError  <- true
            startInfo.UseShellExecute        <- false
            startInfo.WorkingDirectory       <- directory
            let prs = Process.Start startInfo
            let output = prs.StandardOutput.ReadToEnd()
            let error = prs.StandardError.ReadToEnd()
            prs.WaitForExit()
            
            if prs.ExitCode = successExitCode
            then
                output.Trim() |> Ok
            else
                error.Trim()
                |> GitProcessErrorExit
                |> Git
                |> Error
        with
        | exn ->
            exn
            |> RepoAccessFailed
            |> Git
            |> Error
            
    let private toGitCommitInfo (hashMessageWhenCommitString: string) =
        if not (String.IsNullOrWhiteSpace hashMessageWhenCommitString)
        then
            let parts = hashMessageWhenCommitString.Split('|')
            if parts.Length >= 3 then
                match DateTimeOffset.TryParse(parts[2]) with
                | true, dateTimeOffset -> 
                    Some { Hash = parts[0]
                           Message = parts[1]
                           When = dateTimeOffset }
                | _ -> None
            else None
        else None        
    
    let private gitDirectory directory =
        let gitPath = System.IO.Path.Combine(directory, ".git")
        System.IO.Directory.Exists gitPath || System.IO.File.Exists gitPath
            
    let private getStatus
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess "status --porcelain"
            
    let private getBranch
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! branch = gitProcess "branch --show-current"
            if not (String.IsNullOrWhiteSpace branch)
                then return Some branch
                else return None
        }    
        
    let private getSignature
        (timeStamp: DateTimeOffset)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! email = gitProcess "config user.email"
            let! name  = gitProcess "config user.name"
            if (not (String.IsNullOrWhiteSpace email) &&
                not (String.IsNullOrWhiteSpace name))
            then
                return Some { Email = email; Name = name; When = timeStamp }
            else return None
        }
        
    let private getCommit
        (name: string option)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! commit =
                match name with
                | Some n when not (String.IsNullOrWhiteSpace n) ->
                    gitProcess $"log -1 --format=%%H|%%s|%%ci {n}"
                | _ -> gitProcess "log -1 --format=%H|%s|%ci"
            return toGitCommitInfo commit
        }
        
    let private listTags
        (filter: string list)
        (qty: int)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let tagFilter = 
                if filter.IsEmpty
                then ""
                else
                    filter
                    |> List.map (fun prefix -> $"--list \"{prefix}*\"")
                    |> String.concat " "
            
            let! output = gitProcess $"tag --sort=-creatordate {tagFilter}"
            if (not (String.IsNullOrWhiteSpace output))
            then
                let tagNames = output.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                return!
                    tagNames
                    |> Array.take (min qty tagNames.Length)
                    |> Array.toList
                    |> List.traverseResultM (fun tagName ->
                        result {
                            let! c = gitProcess |> getCommit (Some tagName)
                            return { Name = tagName; Commit = c }
                        })
            else return []
        }
        
    let private listCommits
        (tagName: string option)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! output =
                match tagName with
                | Some t ->
                    gitProcess $"log {t}..HEAD --pretty=format:%%H|%%s|%%ci"
                | None ->
                    gitProcess "log --pretty=format:%H|%s|%ci"
                
            if (not (String.IsNullOrWhiteSpace output))
            then
                return
                    output.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                    |> Array.choose toGitCommitInfo                    
                    |> Array.toList                
            else return []
        }
        
    let private isUnborn
        (gitProcess: string -> Result<string,InfrastructureError>) =
        //let head = gitProcess "rev-parse --verify HEAD"
        let head = gitProcess "rev-parse --quiet --verify HEAD^{commit}"
        //Exit 0 ⇒ repo has at least one commit. Non-zero (typically 128) ⇒ unborn.
        
        match head with
        | Error (Git (GitProcessErrorExit _)) -> Ok true
        | Error e -> Error e 
        | Ok _ -> Ok false
   
    let private unstage
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess "reset HEAD ."
        
    let private stage
        (files: string list)
        (gitProcess: string -> Result<string,InfrastructureError>) =
            let files = files |> String.concat " "
            gitProcess $"add {files}"
            
    let private commit
        (commitMessage: string)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess $"commit -m \"{commitMessage}\""
        
    let private tag
        (tagName: string)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess $"tag \"{tagName}\""
     
    let read
        (directory: string)
        (maxTagsToRead: byte)
        (tagsPrefixesToFilter: string list)
        (timeStamp: DateTimeOffset)=
        result {
            if not (gitDirectory directory)
            then
                return None
            else
                let git = runGit directory
                
                let! unborn    = git |> isUnborn
                let! status    = git |> getStatus
                let! branch    = git |> getBranch                
                let! signature = git |> getSignature timeStamp                
                let! commit =
                    if unborn
                    then Ok None
                    else git |> getCommit None
                let! tags =
                    if unborn
                    then Ok []
                    else git |> listTags tagsPrefixesToFilter (int maxTagsToRead)
                
                return Some {                    
                    Directory = directory
                    Unborn = unborn
                    Detached = branch.IsNone
                    CurrentBranch = branch
                    CurrentCommit = commit
                    Signature = signature
                    Dirty = not (String.IsNullOrWhiteSpace status)
                    Tags = tags
                }
        }
        
    let list
        (directory: string)
        (fromTagName: string option) =
        result {
            if not (gitDirectory directory)
            then
                return! Error (Git RepoNotInitialized)
            else                   
                let git = runGit directory
                return! git |> listCommits fromTagName                
        }
        
    let apply
        (directory: string)
        (files: string list)
        (commitMessage: string)
        (tagName: string)=
        result {
            if not (gitDirectory directory)
            then
                return! Error (Git RepoNotInitialized)
            else
                let git = runGit directory
                let! _ = git |> unstage
                let! _ = git |> stage files
                let! _ = git |> commit commitMessage
                let! _ = git |> tag tagName
                return ()
        }

type Git() =
    interface IGit with
        member _.tryGetRepo directory maxTagsToRead tagsPrefixesToFilter timeStamp =
            GitWrapper.read directory maxTagsToRead tagsPrefixesToFilter timeStamp
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryListCommits directory fromTagName=
            GitWrapper.list directory fromTagName
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply (directory, files) commitMessage tagName =
            GitWrapper.apply directory files commitMessage tagName
            |> Result.mapError CalafError.Infrastructure