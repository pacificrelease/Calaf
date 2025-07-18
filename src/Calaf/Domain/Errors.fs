﻿// Domain Errors
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
    
    // Project errors
    | ProjectUnsupported
    
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