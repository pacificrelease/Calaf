namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string
type TagCount = private TagCount of byte

type BumpSettings = {
    ProjectsFindPattern: DotNetXmlFilePattern
    TagsToLoad: TagCount
}

type BumpContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
}

module internal Validation =
    
    let checkDotNetXmlFilePattern (DotNetXmlFilePattern pattern) =
        if System.String.IsNullOrWhiteSpace pattern then Error EmptyDotNetXmlFilePattern
        else Ok <| DotNetXmlFilePattern pattern
        
    let checkTagsToLoad (TagCount count) =
        if count = 0uy then Error ZeroTagCount
        else Ok <| TagCount count

module internal Settings =
    open FsToolkit.ErrorHandling
    
    let tryCreateDotNetXmlFilePattern (filePattern: string) =
        filePattern |> DotNetXmlFilePattern |> Validation.checkDotNetXmlFilePattern
        
    let tryCreateTagCount tagsToLoadCount =
        tagsToLoadCount |> TagCount |> Validation.checkTagsToLoad        
    
    let tryCreate (dotNetXmlFilePattern: string) (tagsToLoadCount: byte) =
        result {
            let! filePattern = dotNetXmlFilePattern |> tryCreateDotNetXmlFilePattern |> Result.mapError CalafError.Validation
            let! tagsToLoadCount = tagsToLoadCount |> tryCreateTagCount |> Result.mapError CalafError.Validation
            return {
                ProjectsFindPattern = filePattern
                TagsToLoad = tagsToLoadCount
            }
        }