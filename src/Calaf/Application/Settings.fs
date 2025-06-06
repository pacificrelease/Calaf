namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string

type TagQuantity = private TagQuantity of byte

type MakeSettings = {
    ProjectsSearchPattern: DotNetXmlFilePattern
    TagsToLoad: TagQuantity
}

module internal Validation =    
    let checkDotNetXmlFilePattern (DotNetXmlFilePattern pattern) =
        if System.String.IsNullOrWhiteSpace pattern then
            EmptyDotNetXmlFilePattern |> CalafError.Validation |> Error
        else pattern |> DotNetXmlFilePattern |> Ok
        
    let checkTagsToLoad (TagQuantity count) =
        if count = 0uy then
            ZeroTagQuantity |> CalafError.Validation |> Error
        else count |> TagQuantity |> Ok

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MakeSettings =
    open FsToolkit.ErrorHandling
    
    let tryCreateDotNetXmlFilePattern (filePattern: string) =
        filePattern |> DotNetXmlFilePattern |> Validation.checkDotNetXmlFilePattern
        
    let tryCreateTagCount tagsToLoadCount =
        tagsToLoadCount |> TagQuantity |> Validation.checkTagsToLoad        
    
    let tryCreate (dotNetXmlFilePattern: string) (tagsToLoadCount: byte) =
        result {
            let! filePattern = dotNetXmlFilePattern |> tryCreateDotNetXmlFilePattern
            let! tagsToLoadCount = tagsToLoadCount  |> tryCreateTagCount
            return {
                ProjectsSearchPattern = filePattern
                TagsToLoad = tagsToLoadCount
            }
        }