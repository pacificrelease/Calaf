namespace Calaf.Application

type MakeConfiguration = {
    TagsTake: byte
    ChangelogFileName: string
    SearchPatterns: string list
}

module MakeConfiguration =
    [<Literal>]
    let private tagsTake = 10uy
    [<Literal>]
    let private changelogFileName = "CHANGELOG.md"
    [<Literal>]
    let private csharpProjectExtension = "*.csproj"
    [<Literal>]
    let private fsharpProjectExtension = "*.fsproj"
    
    let defaultValue =
        { TagsTake = tagsTake
          ChangelogFileName = changelogFileName
          SearchPatterns = [csharpProjectExtension; fsharpProjectExtension] }