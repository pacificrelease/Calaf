namespace Calaf.Tests.PoliciesTests

open Xunit
open Swensen.Unquote

open Calaf.Domain.Policies

module MakePolicyTests =
    [<Fact>]
    let ``Default policy creates expected values`` () =
        let policy = MakePolicy.defaultPolicy
        test <@ policy.TagsTake = MakePolicy.defaultTagsTake &&
                policy.ChangelogFileName = MakePolicy.defaultChangelogFileName &&
                policy.SearchPattern = MakePolicy.defaultSearchPattern @>