// Impure
namespace Calaf

open Calaf.Domain.Errors

module internal Xml =        
    let TryLoadXml (absolutePath: string) =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)            
            xml
            |> Ok
        with exn ->
            exn
            |> CannotLoadXml
            |> Xml
            |> Error
            
    
    let TrySaveXml (absolutePath: string) (xml: System.Xml.Linq.XElement) =
        try
            let options = System.Xml.Linq.SaveOptions.None
            xml.Save(absolutePath, options)
            xml
            |> Ok
        with exn ->
            exn
            |> CannotSaveXml
            |> Xml
            |> Error