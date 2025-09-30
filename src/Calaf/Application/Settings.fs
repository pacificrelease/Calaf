namespace Calaf.Application

type TagQuantity = private TagQuantity of byte
type ChangelogFileName = private ChangelogFileName of string


type MakeSettings = {
    TagsToLoad: TagQuantity
    ChangelogFileName: ChangelogFileName
}

module internal Validation =
    let sanitizeDotNetXmlFilePatterns (searchPatterns: string list) =
        searchPatterns
        |> List.filter (fun p -> not (System.String.IsNullOrWhiteSpace p))
        |> List.distinct
        
    let checkDotNetXmlFilePatterns (searchPatterns: string list) =
        if List.isEmpty searchPatterns then
            EmptyDotNetXmlFilePattern |> CalafError.Validation |> Error
        else Ok searchPatterns
        
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
    
    let tryCreateDotNetXmlFilePatterns (searchPatterns: string list) =
        searchPatterns
        |> Validation.sanitizeDotNetXmlFilePatterns
        |> Validation.checkDotNetXmlFilePatterns
        
    let tryCreateTagCount tagsToLoadCount =
        tagsToLoadCount
        |> Validation.checkTagsToLoad
        |> Result.map TagQuantity
        
    let tryCreateChangelogFileName changelogFileName =
        changelogFileName
        |> Validation.checkFileName
        |> Result.map ChangelogFileName   
    
    let tryCreate
        (tagsToLoadCount: byte)
        (changelogFileName: string) =
        result {
            let! tagsToLoadCount = tryCreateTagCount tagsToLoadCount
            let! changelogFileName = tryCreateChangelogFileName changelogFileName
            return {
                TagsToLoad = tagsToLoadCount
                ChangelogFileName = changelogFileName
            }
        }