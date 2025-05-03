module internal Calaf.Domain.Workspace

open System.IO

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option, projects: Project seq) : Workspace =
    { Directory  = directory.FullName
      Repository = repoInfo |> Option.map Repository.create
      Suite      = projects |> Seq.toArray |> Suite.create }