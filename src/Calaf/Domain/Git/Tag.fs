module internal Calaf.Domain.Tag

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities

let create (tagInfo: GitTagInfo) =
    match Version.tryParseFromTag tagInfo.Name with
    | Some v when v.IsCalVer || v.IsSemVer ->
        let commit = tagInfo.Commit |> Option.map Commit.create
        Tag.Versioned { Name = tagInfo.Name; Version = v; Commit = commit }
    | _ ->
        Tag.Unversioned tagInfo.Name
        
let chooseCalendarVersions (tags: Tag seq) : CalendarVersion seq =
    tags
    |> Seq.choose (function
        | Tag.Versioned { Version = CalVer version } -> Some version
        | _ -> None)