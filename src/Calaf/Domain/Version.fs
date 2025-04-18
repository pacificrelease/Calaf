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
        | [| first; second; third |] ->
            let major   = SemVer.tryParseFromString<Major> first
            let minor   = SemVer.tryParseFromString<Minor> second
            let patch   = SemVer.tryParseFromString<Patch> third
            
            let year    =
                match major with
                | Some major -> major |> int32 |> Year.tryParseFromInt32
                | _ -> Year.tryParseFromString first
            let month =
                match minor with
                | Some minor -> minor |> int32 |> Month.tryParseFromInt32
                | _ -> second |> Month.tryParseFromString 
            let patch  =
                match patch with
                | Some patch -> Some patch
                | _ -> third |> Patch.tryParseFromString 
            
            match year, month, patch with
            | Some year, Some month, patch ->
                return CalVer({ Year = year; Month = month; Patch = patch })            
            | _ ->
            match major, minor, patch with
                | Some major, Some minor, Some patch ->
                    return LooksLikeSemVer({ Major = major; Minor = minor; Patch = patch })
                | _ ->
                    return Unsupported
        | [| year; month |] ->
            let year    = Year.tryParseFromString year
            let month = Month.tryParseFromString month
            match year, month with
            | Some year, Some month ->
                return CalVer({ Year = year; Month = month; Patch = None })
            | _ ->
                return Unsupported
        | _ ->
            return Unsupported
    }