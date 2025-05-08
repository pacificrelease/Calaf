module internal Calaf.Domain.Workspace

open System.IO

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let create (directory: DirectoryInfo, repoInfo: GitRepositoryInfo option) : Workspace =
    let projects = 
        directory.Projects |> Array.map Project.tryCreate
    { Directory  = directory.Directory
      Repository = repoInfo |> Option.map Repository.create
      Suite      = projects |> Array.choose id |> Suite.create}