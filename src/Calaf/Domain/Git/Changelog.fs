module internal Calaf.Domain.Changelog

open Calaf.Domain.DomainTypes.Values

// let tryCreate (commits: Commit list) (dateTimeOffsetStamp: System.DateTimeOffset) =
//     if commits.IsEmpty then
//         None
//     else
//         let features, fixes, breakingChanges =
//             commits
//             |> List.fold (fun (features, fixes, breakingChanges) commit ->
//                 match commit with
//                 | Commit.Feature feature -> feature :: features, fixes, breakingChanges
//                 | Commit.Fix fix -> features, fix :: fixes, breakingChanges
//                 | Commit.BreakingChange breakingChange -> features, fixes, breakingChange :: breakingChanges
//             ) ([], [], [])
//
//         Some {
//             DateTimeOffsetStamp = dateTimeOffsetStamp
//             Features = features
//             Fixes = fixes
//             BreakingChanges = breakingChanges
//         }
