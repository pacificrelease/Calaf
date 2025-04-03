namespace rec Calaf

type HomeDirectory = string

type Language =
    | FSharp
    | CSharp
    
type Project = {
    Name : string
    Directory : string
    Language : Language    
}

