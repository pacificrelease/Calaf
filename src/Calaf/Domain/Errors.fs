// Domain Errors
namespace Calaf.Domain

type DomainError =
    // Version errors
    | YearOutOfRange
    | YearInvalidInt
    | YearInvalidString    
    | MonthOutOfRange
    | MonthInvalidInt
    | MonthInvalidString
    | BuildInvalidString
    
    // Project errors
    | VersionElementMissing
    | VersionElementUpdateFailed
    
    // Suite
    // create error
    | ProjectSuiteEmpty
    // bump errors
    | CalendarVersionMissing
    
    // Git
    // Head errors
    | BranchNameEmpty
    
    // Repository errors
    | RepositoryPathEmpty
    
    // Repository bump errors
    | RepositoryAlreadyCurrent
    | RepositoryCorrupted
    | RepositoryHeadUnborn
    | RepositoryUnsigned
    
    // Workspace
    | WorkspaceAlreadyCurrent