module Calaf.Tests.Integration.FileSystemTests

open Swensen.Unquote
open Xunit

open Calaf.Application
open Calaf.Infrastructure

module TryReadTests =
    [<Fact>]
    let ``tryReadMarkdown returns correct markdown file with expected info`` () =
        let absPath = "D:\StripeProvider\CHANGELOG.md"
        
        let fs = FileSystem() :> IFileSystem
        
        let md = fs.tryReadMarkdown absPath
        let isOk = md.IsOk
        
        test <@ isOk @>