namespace Calaf.Tests.CliTests

open Swensen.Unquote
open Xunit

open Calaf.Contracts
open Calaf.Cli
open Calaf.CliError

module CommandTests =
    [<Fact>]
    let ``Create make stable command from arguments represents Make command with RC MakeType and with the changelog and prerelease inclusion`` () =
        let args = [| "make"; "stable"; "--changelog"; "--include-prerelease" |]
        let commandResult = tryCreateCommand args
        let isOk =
            match commandResult with
            | Ok cmd ->
                cmd = Command.Make { Type = MakeType.Stable; Changelog = true; IncludePreRelease = true }
            | _ -> false
        test <@ isOk @>
        
    [<Fact>]
    let ``Create make stable command with the changelog from arguments represents Make command with Stable MakeType and changelog`` () =
        let args = [| "make"; "stable"; "--changelog" |]
        let commandResult = tryCreateCommand args
        let isOk =
            match commandResult with
            | Ok cmd ->
                cmd = Command.Make { Type = MakeType.Stable; Changelog = true; IncludePreRelease = false }
            | _ -> false
        test <@ isOk @>
        
    [<Fact>]
    let ``Create make stable command from arguments represents Make command with Stable MakeType and no changelog`` () =
        let args = [| "make"; "stable" |]
        let commandResult = tryCreateCommand args
        let isOk =
            match commandResult with
            | Ok cmd ->
                cmd = Command.Make { Type = MakeType.Stable; Changelog = false; IncludePreRelease = false }
            | _ -> false
        test <@ isOk @>
        
    [<Fact>]
    let ``Create make nightly command from arguments represents Make command with Nightly MakeType and no changelog`` () =
        let args = [| "make"; "nightly" |]
        let commandResult = tryCreateCommand args
        let isOk =
            match commandResult with
            | Ok cmd ->
                cmd = Command.Make { Type = MakeType.Nightly; Changelog = false; IncludePreRelease = false }
            | _ -> false
        test <@ isOk @>
    
    [<Fact>]
    let ``Create make stable command from bad arguments represents error response`` () =
        let args = [| "not-exist"; "stable" |]
        let commandResult = tryCreateCommand args
        let correctErrorResponse =
            match commandResult with
            | Error r ->
                r.ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode &&
                not (System.String.IsNullOrWhiteSpace r.Text)
            | _ -> false
        test <@ correctErrorResponse @>
        
    [<Fact>]
    let ``Create make nightly command from bad arguments represents error response`` () =
        let args = [| "not-exist"; "nightly" |]
        let commandResult = tryCreateCommand args
        let correctErrorResponse =
            match commandResult with
            | Error r ->
                r.ExitCode = MisuseShellCommandOrInvalidArgumentsExitCode &&
                not (System.String.IsNullOrWhiteSpace r.Text)
            | _ -> false
        test <@ correctErrorResponse @>