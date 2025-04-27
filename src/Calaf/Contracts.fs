namespace Calaf.Contracts

type TagsQuantity = byte

type GitCommitInfo = {
    Hash: string
    Message: string
    When: System.DateTimeOffset
}

type GitTagInfo = {
    Name: string
    Commit: GitCommitInfo option
}

type GitRepositoryInfo = {
    Directory: string
    Dirty: bool
    HeadDetached: bool
    CurrentBranch: string
    Tags: GitTagInfo[]
}