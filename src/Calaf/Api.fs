module internal Calaf.Api

open FsToolkit.ErrorHandling

open Calaf.Domain.DomainTypes
open Calaf.Domain.Errors
open Calaf.Domain

module Projects =    
    let load projectFileInfo =
        let createProject metadata xml =            
            match Project.tryCreate xml metadata with
            | None -> CannotCreateProject metadata.Name |> Init |> Error
            | Some project -> (project, xml) |> Ok
            
        result {
            let metadata = ProjectMetadata.create projectFileInfo            
            let! xml = Xml.tryLoadXml(metadata.AbsolutePath)            
            return! createProject metadata xml
        }
        
    let bump (project, xml) newVersion =        
        result {            
            let! bumped, xml = Project.tryBump xml project newVersion
            return (bumped, xml)
        }
        
    let save (project, xml) =
        result {
            match project with
            | Bumped (metadata, _, _, _) ->
                let! xml = Xml.trySaveXml metadata.AbsolutePath xml
                return (project, xml)
            | Versioned (metadata, _, _) ->
                return! GivenNotBumpedProject metadata.Name
                |> Api
                |> Error
            | Skipped (metadata, _, _) ->
                return! GivenSkippedProject metadata.Name
                |> Api
                |> Error
            | Unversioned (metadata, _) ->
                return! GivenUnversionedProject metadata.Name
                |> Api
                |> Error
        }