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
        let timeoutMs = 30_000
        let gitCommand = "git"
        
        let start = ProcessStartInfo(gitCommand, $"-c core.pager=cat {arguments}")
        start.RedirectStandardOutput <- true
        start.RedirectStandardError  <- true
        start.StandardOutputEncoding <- Text.Encoding.UTF8
        start.StandardErrorEncoding  <- Text.Encoding.UTF8
        start.UseShellExecute        <- false
        start.WorkingDirectory       <- directory
    
        try    
            use proc = Process.Start start
            let stdoutTask = proc.StandardOutput.ReadToEndAsync()
            let stderrTask = proc.StandardError.ReadToEndAsync()            
            if not (proc.WaitForExit(timeoutMs)) then
                try proc.Kill(true) with _ -> ()
                proc.WaitForExit()
                GitProcessTimeout
                |> Git
                |> Error
            else            
                let out = stdoutTask.Result.TrimEnd('\r','\n')
                let err = stderrTask.Result.TrimEnd('\r','\n')
                
                if proc.ExitCode = 0
                then Ok out
                else
                    let errMsg =
                        if String.IsNullOrWhiteSpace err
                        then $"git {arguments} -> exit code {proc.ExitCode}"
                        else err
                    errMsg
                    |> GitProcessErrorExit
                    |> Git
                    |> Error
        with
        | exn ->
            exn
            |> RepoAccessFailed
            |> Git
            |> Error
            
    let private toGitCommitInfo (hashTextWhenCommitString: string) =
        if not (String.IsNullOrWhiteSpace hashTextWhenCommitString)
        then
            let parts = hashTextWhenCommitString.Split('|')
            if parts.Length >= 3 then
                match DateTimeOffset.TryParse(parts[2]) with
                | true, dateTimeOffset -> 
                    Some { Hash = parts[0]
                           Text = parts[1]
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
        (exclude: string list)
        (qty: int)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        result {                    
            let includes =
                filter
                |> List.choose (fun i ->
                    let i = i.Trim()
                    if String.IsNullOrWhiteSpace i
                    then None
                    else Some $"refs/tags/{i}*")                
            let excludes =
                exclude
                |> List.choose (fun e ->
                    let e = e.Trim()
                    if String.IsNullOrWhiteSpace e
                    then None
                    else Some $"--exclude=refs/tags/{e}")                
            let opts =
                [ "--ignore-case"
                  "--sort=-creatordate"
                  "--format=\"%(refname:short)\"" ]
                @ excludes                
            let args = String.concat " " (opts @ includes)                
            let cmd = $"for-each-ref {args}"
            
            let! out = gitProcess $"{cmd}"
            if String.IsNullOrWhiteSpace out then
                return []
            else
                let names =
                    out.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

                return!
                    names
                    |> Array.truncate (min qty names.Length)
                    |> Array.toList
                    |> List.traverseResultM (fun tag ->
                        result {
                            let! c = gitProcess |> getCommit (Some tag)
                            return { Name = tag; Commit = c }
                        })
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
        let head = gitProcess "rev-parse --quiet --verify HEAD^{commit}"
        
        match head with
        | Error (Git (GitProcessErrorExit _)) -> Ok true
        | Error e -> Error e 
        | Ok _ -> Ok false
   
    let private unstage
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess "reset HEAD ."
        
    let private changes
        (gitProcess: string -> Result<string,InfrastructureError>) =
        
        let parseTokens (tokenString: string) =
            if String.IsNullOrWhiteSpace tokenString then []
            else tokenString.Split('\u0000', StringSplitOptions.RemoveEmptyEntries) |> Array.toList
            
        let extract (record: string) : string list * bool =
            match record with
            | null | "" -> [], false
            | _ ->
                match record[0] with
                | '1' | 'u' ->
                    let idx = record.LastIndexOf ' '
                    if idx >= 0 then [ record.Substring(idx + 1) ], false
                    else [], false

                | '2' ->
                    let idx = record.LastIndexOf ' '
                    if idx >= 0 then [ record.Substring(idx + 1) ], true
                    else [], true      // still consume next to stay in sync

                | '?' ->
                    if record.Length > 2 && record[1] = ' ' then
                        [ record.Substring 2 ], false
                    else [], false

                | '#' | '!' -> [], false
                | _         -> [], false

        result {
            let! root      = gitProcess "rev-parse --show-toplevel"
            let! porcelain = gitProcess "status --porcelain=v2 -z"

            let rec loop acc tokens =
                match tokens with
                | [] -> List.rev acc
                | r :: rest ->
                    let paths, needNext = extract r
                    match needNext, rest with
                    | true, orig :: tail ->
                        loop (orig :: paths @ acc) tail
                    | true, [] ->
                        loop (paths @ acc) [] 
                    | _ ->
                        loop (paths @ acc) rest

            let paths =
                porcelain
                |> parseTokens
                |> loop []
                |> List.distinct
                |> List.map (fun rel -> System.IO.Path.Combine(root, rel) |> System.IO.Path.GetFullPath)

            return paths
        }

        
    let private stage
        (files: string list)
        (changes: string list)
        (gitProcess: string -> Result<string,InfrastructureError>) =            
        result {
            let changesSet = Set.ofList changes
            let selected = files |> List.filter changesSet.Contains

            if selected.IsEmpty then
                return! Error (Git RepoStageNoChanges)
            else
                let! root = gitProcess "rev-parse --show-toplevel"

                let quote (p: string) =
                    let abs =
                        if System.IO.Path.IsPathRooted p
                        then System.IO.Path.GetFullPath p
                        else System.IO.Path.Combine(root, p) |> System.IO.Path.GetFullPath
                    let rel = System.IO.Path.GetRelativePath(root, abs)
                    let relEsc = rel.Replace("\"", "\\\"")
                    $"\"{relEsc}\""

                let args =
                    selected
                    |> List.map quote
                    |> String.concat " "

                return! gitProcess $"add -- {args}"
        }
            
    let private commit
        (commitText: string)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess $"commit -m \"{commitText}\""
        
    let private tag
        (tagName: string)
        (gitProcess: string -> Result<string,InfrastructureError>) =
        gitProcess $"tag \"{tagName}\""
     
    let read
        (directory: string)
        (maxTagsToRead: byte)
        (tagsInclude: string list)
        (tagsExclude: string list option)
        (timeStamp: DateTimeOffset)=
        result {
            if not (gitDirectory directory)
            then return None
            else
                let git = runGit directory
                let maxTags = (int maxTagsToRead)
                
                let! unborn    = git |> isUnborn
                let! status    = git |> getStatus
                let! branch    = git |> getBranch                
                let! signature = git |> getSignature timeStamp
                
                let! commitData =
                    if unborn then Ok (None, [], None)
                    else
                        result {
                            let! commit      = git |> getCommit None
                            let! versionTags = git |> listTags tagsInclude List.Empty maxTags
                            //let! stableVersionTags =  git |> listTags tagsInclude tagsExclude maxTags)
                            let! baselineTags =
                                // tagsExclude
                                // |> Option.filter (fun excludes -> not excludes.IsEmpty)
                                // |> Option.traverseResult (fun excludes -> git |> listTags tagsInclude excludes maxTags)
                                // |> Option.defaultValue (Ok (Some versionTags))
                                
                                // tagsExclude
                                // |> Option.filter (fun excludes -> not excludes.IsEmpty)
                                // |> Option.map (fun excludes -> git |> listTags tagsInclude excludes maxTags |> Result.map Some)
                                // |> Option.defaultValue (Ok (Some versionTags))
                                    
                                match tagsExclude with
                                | Some excludes when not excludes.IsEmpty ->
                                    git |> listTags tagsInclude excludes maxTags |> Result.map Some
                                | Some empty when empty.IsEmpty ->
                                    Ok (Some versionTags)
                                | _ -> Ok None
                            return (commit, versionTags, baselineTags)
                        }
                let commit, versionTags, baselineTags = commitData
                let dirty = not (String.IsNullOrWhiteSpace status)

                return Some {
                    Directory     = directory
                    Unborn        = unborn
                    Detached      = branch.IsNone
                    CurrentBranch = branch
                    CurrentCommit = commit
                    Signature     = signature
                    Dirty         = dirty
                    VersionTags   = versionTags
                    BaselineTags  = baselineTags }
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
        (commitText: string)
        (tagName: string)=
        result {
            if not (gitDirectory directory)
            then
                return! Error (Git RepoNotInitialized)
            else
                let git = runGit directory
                let! _ = git |> unstage
                let! c = git |> changes                
                let! _ = git |> stage files c
                let! _ = git |> commit commitText
                let! _ = git |> tag tagName
                return ()
        }

type Git() =
    interface IGit with
        member _.tryGetRepo directory maxTagsToRead tagsInclude tagsExclude timeStamp =
            GitWrapper.read directory maxTagsToRead tagsInclude tagsExclude timeStamp
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryListCommits directory fromTagName=
            GitWrapper.list directory fromTagName
            |> Result.mapError CalafError.Infrastructure
            
        member _.tryApply (directory, files) commitText tagName =
            GitWrapper.apply directory files commitText tagName
            |> Result.mapError CalafError.Infrastructure