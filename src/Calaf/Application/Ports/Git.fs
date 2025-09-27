namespace Calaf.Application

open Calaf.Contracts

type IGit =
    abstract tryGetRepo:
        directory: string ->
        maxTagsToRead: byte ->
        tagsInclude: string list ->
        tagsExclude: string list option ->
        timeStamp: System.DateTimeOffset -> Result<GitRepositoryInfo option, CalafError>
        
    abstract tryListCommits:
        directory: string ->
        fromTagName: string option -> Result<GitCommitInfo list, CalafError>
        
    abstract tryApply:
        directory: string * files: string list ->
        commitText: string ->
        tagName: string -> Result<unit, CalafError>