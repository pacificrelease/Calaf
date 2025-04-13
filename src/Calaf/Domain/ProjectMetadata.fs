namespace Calaf

open System.IO

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal ProjectMetadata =
    let create (fileInfo: FileInfo) : ProjectMetadata =
        { Name = fileInfo.Name            
          Directory = fileInfo.DirectoryName
          AbsolutePath = fileInfo.FullName
          Extension = fileInfo.Extension }