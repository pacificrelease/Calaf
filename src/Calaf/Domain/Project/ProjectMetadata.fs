module internal Calaf.Domain.ProjectMetadata

open System.IO

open Calaf.Domain.DomainTypes

let create (fileInfo: FileInfo) : ProjectMetadata =
    { Name = fileInfo.Name            
      Directory = fileInfo.DirectoryName
      AbsolutePath = fileInfo.FullName
      Extension = fileInfo.Extension }