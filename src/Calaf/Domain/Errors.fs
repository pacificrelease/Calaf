// Domain Errors
namespace Calaf.Domain

// TODO: Dedicate 3 main groups of errors:
// 1. Adapters/validation errors |> create values on domain boundaries
// 2. Domain errors |> domain logic errors
// 3. Infrastructure errors |> IO errors

type DomainError =
    | OutOfRangeYear
    | OutOfRangeMonth
    | WrongInt32Year
    | WrongInt32Month
    | WrongStringYear
    | WrongStringMonth
    
    | XElementUpdateFailure of name: string
    | UnversionedProject
    
    | CannotCreateProject of path: string
    | NoCalendarVersionProject
    
    
    
    | GivenNotBumpedProject of name: string
    | GivenUnversionedProject of name: string
    | GivenSkippedProject of name: string
    | NoPropertyGroupWorkspaceVersion
    | NoPropertyGroupNextVersion