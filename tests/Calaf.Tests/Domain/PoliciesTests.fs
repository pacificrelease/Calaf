namespace Calaf.Tests.PoliciesTests

open Xunit
open Swensen.Unquote

open Calaf.Domain.Make.Policy

module MakePolicyTests =
    [<Fact>]
    let ``Default policy creates expected values`` () =
        let policy = defaultPolicy
        test <@ policy.TagsTake = defaultTagsTake &&
                policy.ChangelogFileName = defaultChangelogFileName &&
                policy.SearchPattern = defaultSearchPattern @>