namespace Calaf.Application

open Calaf.Contracts

type IGit =
    abstract tryGetRepo:
        directory: string ->
        maxTagsToRead: byte ->
        tagsPrefixesToFilter: string list ->
        timeStamp: System.DateTimeOffset -> Result<GitRepositoryInfo option, CalafError>
        
    abstract tryListCommits:
        directory: string ->
        tagName: string option -> Result<string, CalafError>
        
    abstract tryApply:
        directory: string * files: string list ->
        commitMessage: string ->
        tagName: string -> Result<unit, CalafError>