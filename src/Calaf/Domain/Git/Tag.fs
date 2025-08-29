module internal Calaf.Domain.Tag

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (tagInfo: GitTagInfo) =
    match Version.tryParseFromTag tagInfo.Name with
    | Some v when v.IsCalVer || v.IsSemVer ->
        let commit = tagInfo.Commit |> Option.map Commit.create
        Tag.Versioned { Name = tagInfo.Name; Version = v; Commit = commit }
    | _ ->
        Tag.Unversioned tagInfo.Name
    
let chooseCalendarVersions (tags: Tag seq) : RepositoryVersion seq =
    tags
    |> Seq.choose (function
        | Tag.Versioned { Name = tagName; Version = CalVer version; Commit = commit } ->
            Some { TagName = tagName
                   Version = CalVer version
                   CommitMessage = commit |> Option.map _.Message }
        | _ -> None)