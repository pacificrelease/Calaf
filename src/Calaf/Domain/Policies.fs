namespace Calaf.Domain.Policies

type MakePolicy = {
    TagsTake: byte
    ChangelogFileName: string
    SearchPattern: string
}

module internal MakePolicy =
    [<Literal>]
    let defaultTagsTake = 10uy
    [<Literal>]
    let defaultChangelogFileName = "CHANGELOG.md"
    [<Literal>]
    let defaultSearchPattern = "*.?sproj"
    
    let defaultPolicy : MakePolicy = {
        TagsTake = defaultTagsTake
        ChangelogFileName = defaultChangelogFileName
        SearchPattern = defaultSearchPattern
    }