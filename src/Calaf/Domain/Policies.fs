namespace Calaf.Domain.Make

type Policy = {
    TagsTake: byte
    ChangelogFileName: string
    SearchPattern: string
}

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Policy =
    [<Literal>]
    let defaultTagsTake = 10uy
    [<Literal>]
    let defaultChangelogFileName = "CHANGELOG.md"
    [<Literal>]
    let defaultSearchPattern = "*.?sproj"
    
    let defaultPolicy : Policy = {
        TagsTake = defaultTagsTake
        ChangelogFileName = defaultChangelogFileName
        SearchPattern = defaultSearchPattern
    }