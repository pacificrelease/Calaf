namespace Calaf.Tests.Integration.Git2Tests

open Swensen.Unquote
open Xunit

open Calaf.Contracts
open Calaf.Domain.Version
open Calaf.Application
open Calaf.Infrastructure

module TryReadTests =
    [<Fact(Skip = "TBD")>]
    let ``Test 1`` () =
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
        
    [<Fact(Skip = "TBD")>]
    let ``Test 2`` () =
        let dir = "../../../../.."
        let absPath = System.IO.DirectoryInfo(dir)
        let dir = absPath.FullName
        
        let commitName = "commit name"
        let tagName = "tag name"
        let timeStamp = System.DateTimeOffset.UtcNow
                
        let git2 = Git2() :> IGit
        
        let info = git2.tryApply (dir, []) commitName tagName { Email = "email@email.me"; Name = "Name"; When = timeStamp }
        let isOk = info.IsOk
        
        test <@ isOk @>