module Calaf.Tests.Generator.Contracts

open System.Xml.Linq

open Calaf.Contracts
open Calaf.Domain.Project.XmlSchema
open Calaf.Application
  
let private projectFileExtension = 
    Bogus.Faker().Random.ArrayElement(
        [| Calaf.Domain.Language.FSharpProjExtension; Calaf.Domain.Language.CSharpProjExtension |])

let projectXElement (version : string option) : XElement =
     match version with
     | Some v -> 
         XElement(XName.Get(ProjectXElementName),
            XElement(XName.Get(PropertyGroupXElementName),
                XElement(XName.Get(VersionXElementName), v)))
     | None ->
        XElement(XName.Get(ProjectXElementName),
            XElement(XName.Get(PropertyGroupXElementName)))
     
let projectXmlFileInfo (rootDir: string, version : string option) : ProjectXmlFileInfo =
    let dir =
        if Bogus.Faker().Random.Bool()
        then rootDir + Bogus.Faker().System.DirectoryPath()
        else rootDir
    let name = Bogus.Faker().System.FileName()
    let ext = projectFileExtension
    let absolutePath = dir + "/" + name + ext
    {
        Name = name
        Directory = dir
        Extension = ext
        AbsolutePath = absolutePath
        Content = projectXElement version
    }
     
let directoryInfo () : DirectoryInfo =
    let dir = Bogus.Faker().System.DirectoryPath()
    let projects =
        Bogus.Faker().Make<ProjectXmlFileInfo>(
            int (Bogus.Faker().Random.Byte(1uy, System.Byte.MaxValue)),
            fun (_: int) -> projectXmlFileInfo (dir, Some "2025.7")) |> Seq.toList            
    { Directory = dir; Projects = projects }
    
let makeSettings () : MakeSettings =
    let projectPattern = MakeSettings.tryCreateDotNetXmlFilePattern "*.csproj"
    let tagCount = MakeSettings.tryCreateTagCount 10uy
    match projectPattern, tagCount with
    | Ok pattern, Ok count -> 
        { ProjectsSearchPattern = pattern; TagsToLoad = count }
    | _ -> failwith "Failed to create test settings"