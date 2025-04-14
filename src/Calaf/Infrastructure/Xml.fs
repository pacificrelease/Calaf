// Impure
namespace Calaf

open Calaf.Domain.Errors

module internal Xml =        
    let TryLoadXml (absolutePath: string) : Result<System.Xml.Linq.XElement, XmlError> =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)            
            Ok(xml)
        with exn ->
            exn
            |> ReadXmlError
            |> Error