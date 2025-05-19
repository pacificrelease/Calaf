module internal Calaf.Domain.Language

open Calaf.Domain.DomainTypes.Values
 
 let tryParse ext : Language option =
    match ext with
    | ".fsproj" -> Language.FSharp |> Some
    | ".csproj" -> Language.CSharp |> Some
    | _         -> None