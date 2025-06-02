module Calaf.Application.Output

open Calaf.Domain.DomainTypes.Entities

// TODO: Replace domain types from output
let run (result: Result<Workspace, CalafError>) (console: IConsole) =
    match result with
    | Error error ->
        console.error $"{error}"
        result
        
    | Ok workspace ->
        match workspace.Repository, workspace.Suite with
        | Some _, Suite.StandardSet (version, _ ) ->
            console.write $"Workspace: {workspace.Directory}."
            console.write "Git repository found. Skipping now..."
            console.success $"Current Suite version is {version}. 🚀. \n"
            result
                
        | None, Suite.StandardSet (version, _) ->
            console.write $"Workspace: {workspace.Directory}."
            console.write "Git repository not found."
            console.success $"Current Suite version is {version}. 🚀. \n"
            result