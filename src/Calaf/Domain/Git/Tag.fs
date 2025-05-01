module internal Calaf.Domain.Tag

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let chooseCalendarVersions (tags: Tag seq) : CalendarVersion seq =
    tags
    |> Seq.choose (function
        | Tag.Versioned (_, CalVer version, _) -> Some version
        | _ -> None)
        
let create (tagInfo: GitTagInfo) =
    match Version.tryParseFromTag tagInfo.Name with
    | Some version ->
        let commit = tagInfo.Commit |> Option.map Commit.create
        Tag.Versioned (tagInfo.Name, version, commit)
    | None ->
        Tag.Unversioned tagInfo.Name