namespace Calaf.Infrastructure

module internal Mappings =
    
    open Calaf.Contracts
         
    // FileSystem
    let toProjectXmlFileInfo (file: System.IO.FileInfo) (xml: System.Xml.Linq.XElement) : ProjectXmlFileInfo =
        { Name = file.Name
          Directory = file.DirectoryName
          Extension = file.Extension
          AbsolutePath = file.FullName
          Content = xml }
        
    let toWorkspaceDirectoryInfo (directoryInfo: System.IO.DirectoryInfo) (projectsInfos: (System.IO.FileInfo * System.Xml.Linq.XElement) seq) : DirectoryInfo =
        let projectsInfos = 
            projectsInfos
            |> Seq.map (fun (fileInfo, xml) -> toProjectXmlFileInfo fileInfo xml)
            |> Seq.toList
        { Directory = directoryInfo.FullName
          Projects = projectsInfos }
            
