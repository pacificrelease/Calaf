namespace Calaf.Application

type IClock =
    abstract utcNow:
        unit -> System.DateTimeOffset