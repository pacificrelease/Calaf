module internal Calaf.Domain.ReleaseNotes

open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

module Events =
    let toRepositoryReleaseNotesCaptured releaseNotes =        
        { Notes = releaseNotes }
        |> RepositoryEvent.ReleaseNotesCaptured
        |> DomainEvent.Repository

[<Literal>]
let private Header2 = "##"
[<Literal>]
let private Header3 = "###"
[<Literal>]
let private Bold = "**"
[<Literal>]
let private Colon = ":"
[<Literal>]
let private Whitespace = " "
[<Literal>]
let private Dash = "-"

let private addEmptyLine
    (stringBuilder : System.Text.StringBuilder) =
    stringBuilder.AppendLine System.String.Empty
    
let private addVersionHeader
    (stringBuilder : System.Text.StringBuilder)
    (calendarVersion : CalendarVersion)=
    let versionHeader  = $"{Header2} {Version.toString calendarVersion}"    
    stringBuilder.AppendLine versionHeader
    
let private addTimeStampLine
    (stringBuilder : System.Text.StringBuilder)
    (timeStamp : System.DateTimeOffset)=
    let timeStampLine = timeStamp.ToString("yyyy-MMMM-dd")
    stringBuilder.AppendLine timeStampLine
    
let private addHeaderLine
    (headerText: string) 
    (stringBuilder : System.Text.StringBuilder)
    =
    let breakingChangesHeader = $"{Header3}{Whitespace}{headerText}"
    stringBuilder.AppendLine breakingChangesHeader
    
let private addConventionalCommitMessageLine
    (stringBuilder : System.Text.StringBuilder)
    (ccm: ConventionalCommitMessage) =
    let ccmLine =
        match ccm.Scope with
        | Some scope ->
            $"{Dash}{Whitespace}{Bold}{scope}{Bold}{Colon}{Whitespace}{ccm.Description}"
        | None ->
            $"{Dash}{Whitespace}{ccm.Description}"
    stringBuilder.AppendLine ccmLine
    
let private addCommitTextLine
    (stringBuilder : System.Text.StringBuilder)
    (other: CommitText) =
    let otherLine
        = $"{Dash}{Whitespace}{other}"
    stringBuilder.AppendLine otherLine
    
let private addConventionalCommits
    (stringBuilder : System.Text.StringBuilder)
    (commits: ConventionalCommitMessage list)
    (addHeader: System.Text.StringBuilder -> System.Text.StringBuilder)
    (addLine: System.Text.StringBuilder -> ConventionalCommitMessage -> System.Text.StringBuilder)
    =
    match commits with
    | [] -> stringBuilder
    | _ ->
        let stringBuilder =
            stringBuilder
            |> addEmptyLine
            |> addHeader
            |> addEmptyLine
        commits
        |> List.fold addLine stringBuilder
        
let private addCommits
    (stringBuilder : System.Text.StringBuilder)
    (commits: CommitText list)
    (addHeader: System.Text.StringBuilder -> System.Text.StringBuilder)
    (addLine: System.Text.StringBuilder -> CommitText -> System.Text.StringBuilder)
    =
    match commits with
    | [] -> stringBuilder
    | _ ->
        let stringBuilder =
            stringBuilder
            |> addEmptyLine
            |> addHeader
            |> addEmptyLine
        commits
        |> List.fold addLine stringBuilder
    
let toString
    (releaseNotes: ReleaseNotes) =
    let addSection headerText commits addLine =
        let addHeaderLine = addHeaderLine headerText
        match commits with
        | [] -> id
        | _ -> fun sb -> addConventionalCommits sb commits addHeaderLine addLine
        
    match releaseNotes with
    | WithChanges { Features = features; Fixes = fixes; BreakingChanges = breakingChanges; Version = calendarVersion; TimeStamp = timeStamp } ->
        System.Text.StringBuilder()
        |> addVersionHeader <| calendarVersion
        |> addTimeStampLine <| timeStamp
        |> addEmptyLine
        |> addSection "Features" features addConventionalCommitMessageLine
        |> addSection "Fixed" fixes addConventionalCommitMessageLine
        |> addSection "Breaking Changes" breakingChanges addConventionalCommitMessageLine
        |> addEmptyLine
        |> _.ToString()
    | WithoutChanges (calendarVersion, timeStamp) ->
        System.Text.StringBuilder()
        |> addVersionHeader <| calendarVersion
        |> addTimeStampLine <| timeStamp
        |> addEmptyLine
        |> _.ToString()

let tryCreate
    (commits: Commit list)
    (nextVersion: CalendarVersion)
    (timeStamp: System.DateTimeOffset)=
    let releaseNotes =        
        if commits.IsEmpty then
            WithoutChanges (nextVersion, timeStamp)
        else        
            let categorize (features, fixes, breakingChanges, others) commit =
                match commit.Message with
                | Feature featureMessage when featureMessage.BreakingChange = true ->
                    featureMessage :: features, fixes, featureMessage :: breakingChanges, others                    
                | Feature featureMessage ->
                    featureMessage :: features, fixes, breakingChanges, others
                | Fix fixMessage when fixMessage.BreakingChange = true ->
                    features, fixMessage :: fixes, fixMessage :: breakingChanges, others
                | Fix fixMessage ->
                    features, fixMessage :: fixes, breakingChanges, others
                | Other ct ->
                    features, fixes, breakingChanges, ct :: others
                | Empty ->
                    features, fixes, breakingChanges, others            
            let features, fixes, breakingChanges, others =
                commits |> List.fold categorize ([], [], [], [])
            let releaseChanges = {
                Features = List.rev features
                Fixes = List.rev fixes
                BreakingChanges = List.rev breakingChanges
                Other = others
                Version = nextVersion
                TimeStamp = timeStamp
            }
            ReleaseNotes.WithChanges releaseChanges
    let event = Events.toRepositoryReleaseNotesCaptured releaseNotes
    Some (releaseNotes, [event])