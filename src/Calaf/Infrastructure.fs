namespace Calaf

open System.IO

// Impure
module FileSystem =    
    let readFilesMatching (pattern: string) (workingDir: DirectoryInfo) : FileInfo[] =
        workingDir.GetFiles(pattern, SearchOption.AllDirectories)

module Xml =        
    let tryLoadXml (absolutePath: string) : System.Xml.Linq.XElement option =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)
            Some xml
        with _ ->
            None
            
module Clock =
    let nowUtc () =
        System.DateTime.UtcNow