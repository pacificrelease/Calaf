namespace Calaf.Application.UseCases.Make

open Calaf.Contracts
open Calaf.Application

type Deps = {
    UtcNow:
        unit ->
        System.DateTime
    
    TryReadDirectory:
        string ->
        string list ->
        string ->
        Result<DirectoryInfo, CalafError>        
    TryWriteXml:
        string * System.Xml.Linq.XElement ->
        Result<unit, CalafError>        
    TryWriteMarkdown:
        string * string ->
        Result<unit, CalafError>
        
    TryGetRepo:
        string ->
        byte ->
        string list ->
        string list option ->
        System.DateTimeOffset ->
        Result<GitRepositoryInfo option, CalafError>        
    TryListCommits:
        string ->
        string option ->
        Result<GitCommitInfo list, CalafError>
    TryApply:
        string * string list ->
        string ->
        string ->
        Result<unit, CalafError>
}