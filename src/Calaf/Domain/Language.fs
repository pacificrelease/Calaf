namespace Calaf

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal Language =
     let tryParse ext =
        match ext with
        | ".fsproj" -> Some(Language.FSharp)
        | ".csproj" -> Some(Language.CSharp)
        | _         -> None