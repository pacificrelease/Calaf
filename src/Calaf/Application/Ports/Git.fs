namespace Calaf.Application

open Calaf.Contracts

type IGit =
    abstract tryRead:
        directory: string -> maxTagsToRead: int -> tagsPrefixesToFilter: string list -> timeStamp: System.DateTimeOffset -> Result<GitRepositoryInfo option, CalafError>
        
    abstract tryApply:
        directory: string -> commitMessage: string -> tagName: string -> signature: GitSignatureInfo -> Result<unit, CalafError>
