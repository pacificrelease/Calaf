module internal Calaf.Domain.Version

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes

// TODO: Use ERROR instead of option    
let tryBump (currentVersion: CalendarVersion) (timeStamp: System.DateTime) : CalendarVersion option =
    option {
        let! year = Year.tryParseFromInt32 timeStamp.Year
        let! month = Month.tryParseFromInt32 timeStamp.Month            
        let bumpYear = year > currentVersion.Year            
        if bumpYear then
            return { Year = year
                     Month = month
                     Patch = None }
        else
            let bumpMonth = month > currentVersion.Month
            if bumpMonth then
                return { Year = currentVersion.Year
                         Month = month
                         Patch = None }
            else
                let patch = currentVersion.Patch |> Patch.bump |> Some
                return { Year = currentVersion.Year
                         Month = currentVersion.Month
                         Patch = patch }           
    }   
    
let tryMax (versions: CalendarVersion[]) : CalendarVersion option =
    match versions with
    | [||] -> None
    | _ ->
        let maxVersion = versions |> Array.maxBy (fun v -> v.Year, v.Month, v.Patch)
        Some maxVersion

let tryParse (bareVersion: string) : Version option =
    option {
        let parts = bareVersion.Split('.')
        match parts with
        | [| year; month; patch |] ->
            let year = Year.tryParseFromString year
            let month = Month.tryParseFromString month
            let patch = Patch.tryParseFromString patch
            match year, month, patch with
            | Some year, Some month, patch ->
                return CalVer({ Year = year; Month = month; Patch = patch })
            | _ ->
                return LooksLikeSemVer(bareVersion)
        | [| year; month |] ->
            let year = Year.tryParseFromString year
            let month = Month.tryParseFromString month
            match year, month with
            | Some year, Some month ->
                return CalVer({ Year = year; Month = month; Patch = None })
            | _ ->
                return Unsupported
        | _ ->
            return Unsupported
    }