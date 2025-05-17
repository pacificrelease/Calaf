// Domain Errors
namespace Calaf.Domain

// TODO: Dedicate 3 main groups of errors:
// 1. Adapters/validation errors |> create values on domain boundaries
// 2. Domain errors |> domain logic errors
// 3. Infrastructure errors |> IO errors

// Naming rule: <Verb><Adjective><Noun>
type DomainError =
    // Version errors
    | OutOfRangeYear
    | OutOfRangeMonth
    | WrongInt32Year
    | WrongInt32Month
    | WrongStringYear
    | WrongStringMonth
    
    // FileSystem
    // Project errors
    | NotFoundXmlVersionElement of projectName: string
    
    // Suite
    // create error
    | EmptyProjectsSuite
    // bump errors
    | NoCalendarVersion
    
    // Git
    // Head errors
    | EmptyBranchName
    
    // Repository errors
    | EmptyRepositoryPath
    
    // Repository bump errors
    | CurrentRepository
    | DirtyRepository
    | DamagedRepository
    | UnbornRepository
    | UnsignedRepository