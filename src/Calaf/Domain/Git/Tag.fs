module internal Calaf.Domain.Tag

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (tagInfo: GitTagInfo) =
    match Version.tryParseFromTag tagInfo.Name with
    | Some v when v.IsCalVer || v.IsSemVer ->
        let commit = tagInfo.Commit |> Option.map Commit.create
        Tag.Versioned (tagInfo.Name, v, commit)
    | _ ->
        Tag.Unversioned tagInfo.Name
        
let chooseCalendarVersions (tags: Tag seq) : CalendarVersion seq =
    tags
    |> Seq.choose (function
        | Tag.Versioned (_, CalVer version, _) -> Some version
        | _ -> None)