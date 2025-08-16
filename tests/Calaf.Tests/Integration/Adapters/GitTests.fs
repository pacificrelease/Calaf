namespace Calaf.Tests.Integration.Git2Tests

open Swensen.Unquote
open Xunit

open Calaf.Domain.Version
open Calaf.Application
open Calaf.Infrastructure

module TryReadTests =
    [<Fact(Skip = "TBD")>]
    let ``tryRead returns correct workspace with expected info`` () =
        let dir = "../../../../.."
        let absPath = System.IO.DirectoryInfo(dir)
        let dir = absPath.FullName
        
        let tagsToRead = 10uy
        let prefixes = versionPrefixes
        let timeStamp = System.DateTimeOffset.UtcNow
        
        let git2 = Git() :> IGit
        
        let info = git2.tryGetRepo dir tagsToRead prefixes timeStamp
        let isOk = info.IsOk
        
        test <@ isOk @>
        
    [<Fact(Skip = "TBD")>]
    let ``tryListCommits returns correct list of commits from the previous tag`` () =
        let dir = "../../../../.."
        let absPath = System.IO.DirectoryInfo(dir)
        let dir = absPath.FullName
        
        let tagName = Some "v2025.8"
        
        let git = Git() :> IGit
        
        let commits = git.tryListCommits dir tagName
        let isOk =
            match commits with
            | Ok c -> not c.IsEmpty
            | _ -> false
        
        test <@ isOk @>
        
    [<Fact(Skip = "TBD")>]
    let ``tryApply returns updated expected info `` () =
        let dir = "../../../../.."
        let absPath = System.IO.DirectoryInfo(dir)
        let dir = absPath.FullName
        
        let commitName = "commit name"
        let tagName = "tag name"
                
        let git2 = Git() :> IGit
        
        let info = git2.tryApply (dir, []) commitName tagName
        let isOk = info.IsOk
        
        test <@ isOk @>