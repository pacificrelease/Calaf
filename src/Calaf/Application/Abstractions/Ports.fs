namespace Calaf.Application

open Calaf.Contracts

type IFileSystem =
    abstract tryReadDirectory:
        directory: string ->
        searchPatterns: string list ->
        changelogFilename: string -> Result<DirectoryInfo, CalafError>
        
    abstract tryReadXml:
        absolutePath: string -> Result<System.Xml.Linq.XElement, CalafError>
        
    abstract tryReadMarkdown:
        absolutePath: string -> Result<string option, CalafError>
        
    abstract tryWriteXml:
        absolutePath: string * content: System.Xml.Linq.XElement -> Result<unit, CalafError>
        
    abstract tryWriteMarkdown:
        absolutePath: string * content: string -> Result<unit, CalafError>
        
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
        
type IClock =
    abstract utcNow:
        unit -> System.DateTimeOffset
    
// TODO: Remove an interface and move implementation to Primary (Driving) Adapter
type IConsole =
    abstract member read:
        string[] -> Result<Command, CalafError>
    
    abstract member write:
        string -> unit
        
    abstract member success:
        string -> unit
    
    abstract member error:
        string -> unit