namespace Calaf.Application

type IConsole =
    abstract member write:
        string -> unit
        
    abstract member success:
        string -> unit
    
    abstract member error:
        string -> unit