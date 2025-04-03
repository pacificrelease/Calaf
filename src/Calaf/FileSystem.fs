module rec Calaf.FileSystem

open System.IO

let ListProjects (rootPath: string option) =
    let rootPath = Internals.getRootPathOrDefault rootPath
    
    let searchPattern = "*.?sproj"
    let projectsFiles = Directory.GetFiles(rootPath, searchPattern, SearchOption.AllDirectories)
    
    projectsFiles
    |> Array.choose (fun path ->
        let fileInfo = FileInfo(path)
        Internals.mapProject(fileInfo))
    |> Array.toList
    
    
module private Internals =
    let getRootPathOrDefault rootPath =
        defaultArg rootPath "."
    
    let defineProjectLanguage extension =
        match extension with
        | ".fsproj" -> Some Language.FSharp
        | ".csproj" -> Some Language.CSharp
        | _ -> None
        
    let mapProject (fileInfo: FileInfo)  =
        defineProjectLanguage fileInfo.Extension
        |> Option.map (fun lang -> { Name = fileInfo.Name;  Directory = fileInfo.DirectoryName; Language = lang })