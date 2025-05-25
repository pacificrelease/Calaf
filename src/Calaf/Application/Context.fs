namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string
type TagCount             = private TagCount of byte

type BumpConfig = {
    DotNetXmlFilePattern: DotNetXmlFilePattern
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

module internal Config =
    open FsToolkit.ErrorHandling
    
    let create (filePattern: string) (tagsToLoadCount: byte) =
        result {
            let! filePattern = filePattern |> DotNetXmlFilePattern |> Validation.checkDotNetXmlFilePattern
            let! tagsToLoadCount = tagsToLoadCount |> TagCount |> Validation.checkTagsToLoad
            return {
                DotNetXmlFilePattern = filePattern
                TagsToLoad = tagsToLoadCount
            }
        }
        