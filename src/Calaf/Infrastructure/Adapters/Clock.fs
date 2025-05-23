namespace Calaf.Infrastructure

open Calaf.Application

type Clock() =
    interface IClock with
        member _.now() =
            System.DateTimeOffset.UtcNow