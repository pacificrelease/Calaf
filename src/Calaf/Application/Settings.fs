namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string
type TagQuantity = private TagQuantity of byte
type ChangelogFileName = private ChangelogFileName of string


type MakeSettings = {
    ProjectsSearchPattern: DotNetXmlFilePattern
    TagsToLoad: TagQuantity
    ChangelogFileName: ChangelogFileName
}

module internal Validation =    
    let checkDotNetXmlFilePattern (pattern: string) =
        if System.String.IsNullOrWhiteSpace pattern then
            EmptyDotNetXmlFilePattern |> CalafError.Validation |> Error
        else Ok pattern
        
    let checkTagsToLoad (count: byte) =
        if count = 0uy then ZeroTagQuantity |> CalafError.Validation |> Error
        else Ok count
        
    let checkFileName (fileName: string) =
        match fileName with        
        | f when System.String.IsNullOrWhiteSpace f ->
            EmptyChangelogFileName |> CalafError.Validation |> Error
        | f -> Ok f

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MakeSettings =
    open FsToolkit.ErrorHandling
    
    let tryCreateDotNetXmlFilePattern (filePattern: string) =
        filePattern
        |> Validation.checkDotNetXmlFilePattern
        |> Result.map DotNetXmlFilePattern
        
    let tryCreateTagCount tagsToLoadCount =
        tagsToLoadCount
        |> Validation.checkTagsToLoad
        |> Result.map TagQuantity
        
    let tryCreateChangelogFileName changelogFileName =
        changelogFileName
        |> Validation.checkFileName
        |> Result.map ChangelogFileName   
    
    let tryCreate
        (dotNetXmlFilePattern: string)
        (tagsToLoadCount: byte)
        (changelogFileName: string) =
        result {
            let! filePattern = dotNetXmlFilePattern |> tryCreateDotNetXmlFilePattern
            let! tagsToLoadCount = tagsToLoadCount |> tryCreateTagCount
            let! changelogFileName = tryCreateChangelogFileName changelogFileName
            return {
                ProjectsSearchPattern = filePattern
                TagsToLoad = tagsToLoadCount
                ChangelogFileName = changelogFileName
            }
        }