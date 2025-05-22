namespace Calaf.Application

type IClock =
    abstract now:
        unit -> System.DateTimeOffset