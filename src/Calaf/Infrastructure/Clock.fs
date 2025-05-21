// Impure
namespace Calaf.Infrastructure

open System

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Clock =
    let now () =
        DateTimeOffset.Now
        