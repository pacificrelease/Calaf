namespace Calaf.Infrastructure

open Calaf.Application

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Console =
    let private setForegroundColor color =
        System.Console.ForegroundColor <- color
    
    let private resetForegroundColor () =
        System.Console.ResetColor()
        
    let writeLine (message: string) =
        System.Console.WriteLine(message)
        
    let writeError (message: string) =
        setForegroundColor System.ConsoleColor.Red
        writeLine message
        resetForegroundColor ()
        
    let writeSuccess (message: string) =
        setForegroundColor System.ConsoleColor.Green
        writeLine message
        resetForegroundColor ()    

type Console() =
    interface IConsole with
        member _.write (message: string) =
            message |> Console.writeLine
            
        member _.success (message: string) =
            message |> Console.writeSuccess

        member _.error (message: string) =
            message |> Console.writeError