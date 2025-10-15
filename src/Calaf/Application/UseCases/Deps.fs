namespace Calaf.Application

open Calaf.Contracts
open Calaf.Application

type Deps = {
    //Clock
    UtcNow:
        unit ->
        System.DateTimeOffset
        
    // FileSystem
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
        
    // Git
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
    TryCreateCommit:
        string * string list ->
        string ->
        string ->
        Result<unit, CalafError>
}