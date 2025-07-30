namespace Calaf.Tests.Integration.Git2Tests

open Swensen.Unquote
open Xunit

open Calaf.Domain.Version
open Calaf.Application
open Calaf.Infrastructure

module TryReadTests =
    [<Fact(Skip = "Waiting for the implementation")>]
    let ``Test`` () =
        let dir = "../../../../.."
        let absPath = System.IO.DirectoryInfo(dir)
        let dir = absPath.FullName
        
        let tagsToRead = 10uy
        let prefixes = versionPrefixes
        let timeStamp = System.DateTimeOffset.UtcNow
        
        let git2 = Git2() :> IGit
        
        let info = git2.tryRead dir tagsToRead prefixes timeStamp
        let isOk = info.IsOk
        
        test <@ isOk @>