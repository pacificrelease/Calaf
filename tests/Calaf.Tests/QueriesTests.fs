namespace Calaf.Tests.QueriesTests

open FsCheck.Xunit
open Xunit
open Swensen.Unquote

open Calaf.Application.ProjectsScope
open Calaf.Tests

module TryCreateTests =
    [<Property(Arbitrary = [| typeof<Arbitrary.FileSystem.Directory.Accidental>; typeof<Arbitrary.FileSystem.Directory.DependentRelativeSubDirectoryList> |])>]
    let ``Folders allowed scope returns expected absolute path scope``
        (directory: string)
        (subDirectories: string list)=        
        let scope = tryCreate directory subDirectories
        let isOk =
            match scope with | Ok (Some s) -> s.IsDirectoriesOnly | _ -> false
        test <@ isOk @>
    
    [<Fact>]
    let ``Items allowed and restricted scope returns expected  error`` ()=
        let directory = "/home/users/calaf"
        let badDirectory = "/home/users/calaf2/bad/calaf.fsproj"
        let subDirectories = ["./src"; "./assets"; "/home/users/calaf/tests"; "/home/users/calaf/calaf.csproj"; badDirectory ]
        let scope = tryCreate directory subDirectories
        let isError =
            match scope with
            | Error (Calaf.Application.CalafError.Validation (Calaf.Application.ValidationError.RestrictedProjectPath p)) ->
                p = badDirectory
            | _ -> false           
        test <@ isError @>