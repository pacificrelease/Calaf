module internal Calaf.Domain.SemVer

let inline tryParseFromString<'a when 'a : (static member op_Explicit : uint32 -> 'a) > (versionPart: string) : 'a option =
    match System.UInt32.TryParse versionPart with
    | true, versionPart -> Some ('a.op_Explicit versionPart)
    | _ -> None