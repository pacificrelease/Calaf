namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string

type TagQuantity = private TagQuantity of byte

type BumpSettings = {
    ProjectsSearchPattern: DotNetXmlFilePattern
    TagsToLoad: TagQuantity
}

module internal Validation =    
    let checkDotNetXmlFilePattern (DotNetXmlFilePattern pattern) =
        if System.String.IsNullOrWhiteSpace pattern then Error EmptyDotNetXmlFilePattern
        else Ok <| DotNetXmlFilePattern pattern
        
    let checkTagsToLoad (TagQuantity count) =
        if count = 0uy then Error ZeroTagQuantity
        else Ok <| TagQuantity count

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module BumpSettings =
    open FsToolkit.ErrorHandling
    
    let tryCreateDotNetXmlFilePattern (filePattern: string) =
        filePattern |> DotNetXmlFilePattern |> Validation.checkDotNetXmlFilePattern
        
    let tryCreateTagCount tagsToLoadCount =
        tagsToLoadCount |> TagQuantity |> Validation.checkTagsToLoad        
    
    let tryCreate (dotNetXmlFilePattern: string) (tagsToLoadCount: byte) =
        result {
            let! filePattern = dotNetXmlFilePattern |> tryCreateDotNetXmlFilePattern |> Result.mapError CalafError.Validation
            let! tagsToLoadCount = tagsToLoadCount |> tryCreateTagCount |> Result.mapError CalafError.Validation
            return {
                ProjectsSearchPattern = filePattern
                TagsToLoad = tagsToLoadCount
            }
        }