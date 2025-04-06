module rec Calaf.FileSystem

open System.IO
open FsToolkit.ErrorHandling

let InitWorkspace workingDir =
    let workingDir = FileSystem.workingDirectory workingDir
    let projects = FileSystem.listProjects workingDir
    { Name = workingDir.Name
      Directory = workingDir.FullName
      Projects = projects }
    

module Version =    
    let private tryGetSuitableYearString (year: string) : SuitableString option =
        match year with
        | year when year.Length = 4 &&
                    year |> Seq.forall System.Char.IsDigit
            -> Some year
        | _ -> None    
            
    let private tryGetSuitableMonthString (month: string) : SuitableString option =
        match month with
        | month when (month.Length = 1 || month.Length = 2) &&
                      month |> Seq.forall System.Char.IsDigit
            -> Some month
        | _ -> None
        
    let private toYear (suitableYearString: SuitableString) : Year option =
        match System.UInt16.TryParse(suitableYearString) with
        | true, year -> Some year
        | _ -> None
        
    let private toMonth (suitableMonthString: SuitableString) : Month option =
        match System.Byte.TryParse(suitableMonthString) with
        | true, month -> Some month
        | _ -> None
            
    let private tryParseYear (year: string) : Year option =
        option {
            let! suitableYearString = tryGetSuitableYearString(year)                
            return! toYear suitableYearString
        }
        
    let private tryParseMonth (month: string) : Month option =
        option {
            let! suitableMonthString = tryGetSuitableYearString(month)                
            return! toMonth suitableMonthString
        }
        
    let private tryParsePatch (patch: string) : Patch option =
        match System.UInt32.TryParse(patch) with
        | true, patch -> Some patch
        | _ -> None
        
    let tryGetVersion (version: string)=
        option {
            let parts = version.Split('.')
            match parts with
            | [| year; month; patch |] ->
                let year = tryParseYear year
                let month = tryParseMonth month
                let patch = tryParsePatch patch
                match year, month, patch with
                | Some year, Some month, _ ->
                    return CalVer({ Year = year; Month = month; Patch = patch })
                | _ ->
                    return LooksLikeSemVer(version)
            | [| year; month |] ->
                let year = tryParseYear year
                let month = tryParseMonth month
                match year, month with
                | Some year, Some month ->
                    return CalVer({ Year = year; Month = month; Patch = None })
                | _ ->
                    return Unsupported
            | _ ->
                return Unsupported
        }

module private FileSystem =
    let private defineHomeDirPathOrDefault workingDir =
        defaultArg workingDir "."
        
    let private searchProjects (workingDir: DirectoryInfo) =
        let searchPattern = "*.?sproj"
        workingDir.GetFiles(searchPattern, SearchOption.AllDirectories)

    let private mapProjectLanguage ext =
        match ext with
        | ".fsproj" -> Some(Language.FSharp)
        | ".csproj" -> Some(Language.CSharp)
        | _         -> None
        
    // let private mapVersion (projectMetadata: ProjectMetadata) =
    //     option {
    //         let! version = readVersion projectMetadata.AbsolutePath
    //         return! mapVersion version
    //     }
        
    let mapProject (fileInfo: FileInfo) =
        option {
            let metadata = { Name = fileInfo.Name; Directory = fileInfo.DirectoryName; AbsolutePath = fileInfo.FullName }
            let! language = mapProjectLanguage fileInfo.Extension
            let! versionString = Xml.tryGetVersionString metadata.AbsolutePath
            let version = Version.tryGetVersion versionString
            return
                match version with
                | Some v -> Versioned(metadata, language, v)
                | None   -> Eligible(metadata, language)
        }
        
    let workingDirectory (workingDir : string option) =
        workingDir
        |> defineHomeDirPathOrDefault
        |> DirectoryInfo
        
    let listProjects (workingDir : DirectoryInfo) =    
        searchProjects workingDir
        |> Array.choose mapProject

module private Xml =
    let private readXml (absolutePath: string) =
        let xml = System.Xml.Linq.XElement.Load(absolutePath)
        xml
        
    let private readVersionElements absolutePath =
        option {
            let xml = readXml(absolutePath)
            let versionElements = xml.Elements("PropertyGroup") |> Seq.map _.Elements("Version")
            let version = versionElements |> Seq.tryHead
            return! version
        }              
        
    let tryGetVersionString absolutePath : string option =
        option {
            let! versionElements = readVersionElements absolutePath
            return! match versionElements with
                    | seq when Seq.isEmpty seq -> None
                    | seq when Seq.length seq > 1 -> None
                    | seq -> Some (seq |> Seq.head).Value
        }