namespace Calaf

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal Patch =
    let bump (patch: Patch option) : Patch =
        let increment = 1u
        match patch with
        | Some patch -> (patch + increment)
        | None -> increment
        
    let tryParseFromString (patch: string) : Patch option =
        match System.UInt32.TryParse(patch) with
        | true, patch -> Some patch
        | _ -> None