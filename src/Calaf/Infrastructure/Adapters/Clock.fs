namespace Calaf.Infrastructure

open Calaf.Application

type Clock() =
    interface IClock with
        member _.utcNow() =
            System.DateTimeOffset.UtcNow