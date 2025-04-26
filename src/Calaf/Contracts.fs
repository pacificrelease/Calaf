namespace Calaf.Contracts

type GitTag = {
    Name: string
    When: System.DateTimeOffset option
}

type GitRepository = {
    Directory: string
    Dirty: bool
    HeadDetached: bool
    CurrentBranch: string
    Tags: GitTag[]
}