namespace Calaf.Application

open Calaf.Contracts

type IConsole =
    abstract member read:
        string[] -> Result<Command, CalafError>
    
    abstract member write:
        string -> unit
        
    abstract member success:
        string -> unit
    
    abstract member error:
        string -> unit