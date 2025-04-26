namespace Calaf.Contracts

type GitRepository = {
    Directory: string
    Dirty: bool
    HeadDetached: bool
    CurrentBranch: string    
}