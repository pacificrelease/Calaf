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
    | DayOutOfRange
    | DayInvalidInt
    | BuildInvalidString
    | BuildAlreadyCurrent
    | BuildDowngradeProhibited
    
    // Project errors
    | ProjectUnsupported
    
    | VersionElementMissing
    | VersionElementUpdateFailed
    
    // Collection
    // create error
    | ProjectCollectionEmpty
    | CalendarVersionProjectsEmpty
    // bump errors    
    | CalendarVersionMissing
    
    // Git
    // Head errors
    | BranchNameEmpty
    
    // Repository errors
    | RepositoryPathEmpty
    
    // Repository bump errors
    | RepositoryAlreadyCurrent
    | RepositoryVersionCommitMessageMissing
    | RepositoryCorrupted
    | RepositoryHeadUnborn
    | RepositoryUnsigned
    
    // Workspace
    | WorkspaceAlreadyCurrent