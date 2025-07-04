namespace Calaf.Tests.CliTests

open Swensen.Unquote
open Xunit

open Calaf.Contracts
open Calaf.Cli

module CommandTests =
    [<Fact>]
    let ``Create make stable command from arguments represents Make command with Stable MakeType`` () =
        let args = [| "make"; "stable" |]
        let commandResult = tryCreateCommand args
        test <@ match commandResult with | Ok cmd -> cmd = Command.Make MakeType.Stable | _ -> false @>
        
    [<Fact>]
    let ``Create make nightly command from arguments represents Make command with Nightly MakeType`` () =
        let args = [| "make"; "nightly" |]
        let commandResult = tryCreateCommand args
        test <@ match commandResult with | Ok cmd -> cmd = Command.Make MakeType.Nightly | _ -> false @>
    
    [<Fact>]
    let ``Create make stable command from bad arguments represents error response`` () =
        let args = [| "not-exist"; "stable" |]
        let commandResult = tryCreateCommand args
        test <@ match commandResult with | Error r -> r.ExitCode = 1 && not (System.String.IsNullOrWhiteSpace r.Text) | _ -> false @>
        
    [<Fact>]
    let ``Create make nightly command from bad arguments represents error response`` () =
        let args = [| "not-exist"; "nightly" |]
        let commandResult = tryCreateCommand args
        test <@ match commandResult with | Error r -> r.ExitCode = 1 && not (System.String.IsNullOrWhiteSpace r.Text) | _ -> false @>