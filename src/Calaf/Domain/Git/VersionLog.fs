module internal Calaf.Domain.VersionLog

open Calaf.Domain.DomainTypes
open Calaf.Domain.DomainEvents

module Events =
    let toRepositoryLogCaptured versionLog =        
        { Log = versionLog }
        |> RepositoryEvent.RepositoryLogCaptured
        |> DomainEvent.Repository
        
let tryCreate (commits: Commit list) =
    if commits.IsEmpty then
        None
    else        
        let categorize (features, fixes, breakingChanges) commit =
            match commit.Message with
            | Feature featureMessage when featureMessage.BreakingChange = true ->
                commit.Message :: features, fixes, commit.Message :: breakingChanges                    
            | Feature _ ->
                commit.Message :: features, fixes, breakingChanges                    
            | Fix fixMessage when fixMessage.BreakingChange = true ->
                features, commit.Message :: fixes, commit.Message :: breakingChanges                
            | Fix _ ->
                features, commit.Message :: fixes, breakingChanges                
            | _ -> features, fixes, breakingChanges
        
        let features, fixes, breakingChanges =
            commits
            |> List.fold categorize ([], [], [])

        let versionLog = {
            Features = List.rev features
            Fixes = List.rev fixes
            BreakingChanges = List.rev breakingChanges
        }
        let event = Events.toRepositoryLogCaptured versionLog
        Some (versionLog, [event])