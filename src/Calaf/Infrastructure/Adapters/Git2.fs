namespace Calaf.Infrastructure

open System
open System.Diagnostics
open FsToolkit.ErrorHandling

open Calaf.Contracts
open Calaf.Application

module internal GitRepository2 =    
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
    
    let private gitDirectory directory =
        let gitPath = System.IO.Path.Combine(directory, ".git")
        System.IO.Directory.Exists gitPath || System.IO.File.Exists gitPath
            
    let private status
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            return! gitProcess "status --porcelain"
        }
        
    let private damaged
        (gitProcess: string -> Result<string,InfrastructureError>) =
        let headCheck = gitProcess "rev-parse --verify HEAD"
        let refsCheck = gitProcess "show-ref"        
        match headCheck, refsCheck with
        | Error _, Error _ -> true
        | _ -> false
            
    let private branch
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! branch = gitProcess "branch --show-current"
            if not (String.IsNullOrWhiteSpace branch)
                then return Some branch
                else return None
        }
        
    let private commit
        (name: string option)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {
            let! commit =
                match name with
                | Some n when not (String.IsNullOrWhiteSpace n) ->
                    gitProcess $"log -1 --format=%%H|%%s|%%ci {n}"
                | _ -> gitProcess "log -1 --format=%H|%s|%ci"
            if not (String.IsNullOrWhiteSpace commit)
            then
                let parts = commit.Split('|')
                if parts.Length >= 3 then
                    match DateTimeOffset.TryParse(parts[2]) with
                    | true, w -> 
                        return Some { Hash = parts[0]
                                      Message = parts[1]
                                      When = w }
                    | _ -> return None
                else return None
            else return None
        }
        
    let private signature
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
        
    let private tags
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
                            let! c = gitProcess |> commit (Some tagName)
                            return { Name = tagName; Commit = c }
                        })
            else return []
        }
        
    let private unborn
        (gitProcess: string -> Result<string,InfrastructureError>) =
        let head = gitProcess "rev-parse HEAD"
        match head with
        | Error (Git (GitProcessErrorExit _)) -> Ok true
        | Error e -> Error e 
        | Ok _ -> Ok false
    
    let read
        (directory: string)
        (maxTagsToRead: byte)
        (tagsPrefixesToFilter: string list)
        (timeStamp: DateTimeOffset) =
        result {
            if not (gitDirectory directory)
            then
                return None
            else
                let git = runGit directory            
                let! status    = git |> status
                let! branch    = git |> branch
                let! commit    = git |> commit None
                let! signature = git |> signature timeStamp                
                let! tags      = git |> tags tagsPrefixesToFilter (int maxTagsToRead)
                let! unborn    = git |> unborn
                return Some {                    
                    Directory = directory
                    Damaged = false
                    Unborn = unborn
                    Detached = branch.IsNone
                    CurrentBranch = branch
                    CurrentCommit = commit
                    Signature = signature
                    Dirty = not (String.IsNullOrWhiteSpace status)
                    Tags = tags
                }
        }                

type Git2() =
    interface IGit with
        member _.tryRead directory maxTagsToRead tagsPrefixesToFilter timeStamp =
            GitRepository2.read directory maxTagsToRead tagsPrefixesToFilter timeStamp
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply (directory, files) commitMessage tagName signature =
            () |> Ok