namespace Calaf.Extensions

module internal InternalExtensions =
    
    module Result =
        let partition (results: Result<'a, 'e> seq) =
            results
            |> Seq.fold (fun (oks, errs) result ->
                match result with
                | Ok x -> x :: oks, errs
                | Error e -> oks, e :: errs) ([], [])
            |> fun (oks, errs) ->
                List.rev oks  |> List.toSeq,
                List.rev errs |> List.toSeq  
    
    module Option =        
        let toResult err opt =
            match opt with
            | Some value -> Ok value
            | None       -> Error err