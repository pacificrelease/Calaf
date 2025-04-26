// Impure
namespace Calaf

open System

module internal Clock =
    let Now () =
        DateTimeOffset.Now
        