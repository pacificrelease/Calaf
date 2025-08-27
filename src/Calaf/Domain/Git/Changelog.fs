module internal Calaf.Domain.Changelog

open Calaf.Domain.DomainTypes

let tryCreate (commits: Commit list) =
    if commits.IsEmpty then
        None
    else
        let features, fixes, breakingChanges =
            commits
            |> List.fold (fun (features, fixes, breakingChanges) commit ->
                match commit.Message with
                | Feature (breakingChange, scope, message) when breakingChange = true ->
                    (Feature (breakingChange, scope, message)) :: features, fixes, (Feature (breakingChange, scope, message)) :: breakingChanges                    
                | Feature (breakingChange, scope, message) ->
                    (Feature (breakingChange, scope, message)) :: features, fixes, breakingChanges                    
                | Fix (breakingChange, scope, message) when breakingChange = true ->
                    features, (Fix (breakingChange, scope, message)) :: fixes, (Fix (breakingChange, scope, message)) :: breakingChanges                
                | Fix (breakingChange, scope, message) ->
                    features, (Fix (breakingChange, scope, message)) :: fixes, breakingChanges                
                | _ -> features, fixes, breakingChanges
            ) ([], [], [])

        Some {
            Features = features
            Fixes = fixes
            BreakingChanges = breakingChanges
        }