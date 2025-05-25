namespace Calaf.Application

type BumpContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
}

module internal Validation =
    open Calaf.Contracts
    
    let checkDotNetXmlFilePattern (DotNetXmlFilePattern pattern) =
        if System.String.IsNullOrWhiteSpace pattern then Error EmptyDotNetXmlFilePattern
        else Ok <| DotNetXmlFilePattern pattern
        
    let checkTagsToLoad (TagQuantity count) =
        if count = 0uy then Error ZeroTagQuantity
        else Ok <| TagQuantity count

module internal Settings =
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