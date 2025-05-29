module Calaf.Application.Output

open Calaf.Domain.DomainTypes.Entities

// TODO: Replace domain types from output
let run (result: Result<Workspace, CalafError>) (context: OutputContext) =
    match result with
    | Error error ->
        context.Console.error $"{error}"
        result
        
    | Ok workspace ->
        match workspace.Repository, workspace.Suite with
        | Some _, Suite.StandardSet (version, _ ) ->
            context.Console.write $"Workspace: {workspace.Directory}."
            context.Console.write "Git repository found. Skipping now..."
            context.Console.success $"Current Suite version is {version}. 🚀. \n"
            result
                
        | None, Suite.StandardSet (version, _) ->
            context.Console.write $"Workspace: {workspace.Directory}."
            context.Console.write "Git repository not found."
            context.Console.success $"Current Suite version is {version}. 🚀. \n"
            result