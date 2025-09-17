module internal Calaf.Domain.Language

open Calaf.Domain.DomainTypes

[<Literal>]
let internal FSharpProjExtension = ".fsproj"
[<Literal>]
let internal CSharpProjExtension = ".csproj"
 
let tryParse ext : Language option =
    match ext with
    | FSharpProjExtension -> Language.FSharp |> Some
    | CSharpProjExtension -> Language.CSharp |> Some
    | _         -> None