// Impure
namespace Calaf

open System

module internal Clock =
    let now () =
        DateTimeOffset.Now
        