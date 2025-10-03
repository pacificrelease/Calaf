namespace Calaf.Tests.QueriesTests

open Xunit
open Swensen.Unquote

open Calaf.Application.ProjectsScope

module TryCreateTests =
    [<Fact>]
    let ``Folders allowed scope returns expected absolute path scope`` () =
        let directory = "/home/calaf"
        let folders = [ "./src" ]
        
        let scope = tryCreate directory folders
        let isOk = scope.IsOk
        test <@ isOk @>
    
    [<Fact>]
    let ``Test Error`` () =
        let directory = System.IO.Directory.GetCurrentDirectory()
        let folders = [ "../../../../../src/Calaf" ]
        
        let scope = tryCreate directory folders
        let isError = scope.IsError
        test <@ isError @>