// Impure
namespace Calaf

module internal Xml =        
    let TryLoadXml (absolutePath: string) : System.Xml.Linq.XElement option =
        try
            let xml = System.Xml.Linq.XElement.Load(absolutePath)
            Some xml
        with _ ->
            None

