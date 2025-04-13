namespace Calaf

open FsToolkit.ErrorHandling

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module internal Year =
    let private tryParseYearString (year: string) : SuitableVersionPart option =
        match year with
        | year when year.Length = 4 &&
                    year |> Seq.forall System.Char.IsDigit
            -> Some year
        | _ -> None
        
    let private tryParseYear (suitableYearString: SuitableVersionPart) : Year option =
        match System.UInt16.TryParse(suitableYearString) with
        | true, year -> Some year
        | _ -> None

    // TODO: Use ERROR instead of option
    let tryParseFromInt32 (year: System.Int32) : Year option =
        try
            let year = System.Convert.ToUInt16(year)
            Some year
        with _ ->
            None        

    let tryParseFromString (year: string) : Year option =
        option {
            let! suitableYearString = tryParseYearString(year)                
            return! tryParseYear suitableYearString
        }