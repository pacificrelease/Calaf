// Impure
namespace Calaf.Infrastructure

open System

module internal Clock =
    let now () =
        DateTimeOffset.Now
        