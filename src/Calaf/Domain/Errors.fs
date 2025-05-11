// Domain Errors
namespace Calaf.Domain

// TODO: Dedicate 3 main groups of errors:
// 1. Adapters/validation errors |> create values on domain boundaries
// 2. Domain errors |> domain logic errors
// 3. Infrastructure errors |> IO errors

type DomainError =
    // Version errors
    | OutOfRangeYear
    | OutOfRangeMonth
    | WrongInt32Year
    | WrongInt32Month
    | WrongStringYear
    | WrongStringMonth
    
    // Project errors
    | NotFoundXmlVersionElement of projectName: string
    
    // Suite errors
    | NoProjectsVersion