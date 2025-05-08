namespace Calaf.Infrastructure

open LibGit2Sharp

open Calaf.Infrastructure

module internal Git =
    let tryReadRepository (path: string) (maxTagsToRead: int) =
        try
            if Repository.IsValid(path)
            then
                use repo = new Repository(path)
                let gitRepoInfo = Mappings.toGitRepositoryInfo repo maxTagsToRead
                gitRepoInfo |> Some |> Ok
            else
                None |> Ok
        with exn ->                     
            exn |> RepoAccessFailed |> Git |> Error