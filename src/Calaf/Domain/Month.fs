namespace Calaf

open FsToolkit.ErrorHandling

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal Month =
    let private tryParseMonthString (month: string) : SuitableVersionPart option =
        match month with
        | month when (month.Length = 1 || month.Length = 2) &&
                      month |> Seq.forall System.Char.IsDigit
            -> Some month
        | _ -> None
        
    let private tryParseMonth (suitableMonthString: SuitableVersionPart) : Month option =
        match System.Byte.TryParse(suitableMonthString) with
        | true, month -> Some month
        | _ -> None
        
    let tryParseFromInt32 (month: System.Int32) : Month option =
        try
            let month = System.Convert.ToByte(month)
            Some month
        with _ ->
            None
            
    let tryParseFromString (month: string) : Month option =
        option {            
            let! suitableMonthString = tryParseMonthString(month)                
            return! tryParseMonth suitableMonthString
        }