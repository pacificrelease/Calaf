namespace Calaf

open FsToolkit.ErrorHandling

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal Project =
    let choosePending (projects: Project[]) : Project[] =
        projects
        |> Array.choose (function
            | Versioned (_, _, CalVer _) as project -> Some project
            //| Bumped _ as project -> Some project
            | _ -> None)

    let chooseCalendarVersions (projects: Project[]) : CalendarVersion[] =
        projects
        |> Array.choose (function
            | Versioned (_, _, CalVer version) -> Some version
            | Bumped (_, _, _, version) -> Some version
            | _ -> None)    
        
    let tryCreate (metadata: ProjectMetadata, xml: System.Xml.Linq.XElement) : Project option =
        option {            
            let! language = Language.tryParse metadata.Extension
            let version = Version.tryParse xml
            return
                match version with
                | Some version -> Versioned(metadata, language, version)
                | None   -> Eligible(metadata, language)
        }