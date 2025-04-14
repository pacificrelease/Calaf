module internal Calaf.Domain.Language

open Calaf.Domain.DomainTypes
 
 let tryParse ext =
    match ext with
    | ".fsproj" -> Some(Language.FSharp)
    | ".csproj" -> Some(Language.CSharp)
    | _         -> None