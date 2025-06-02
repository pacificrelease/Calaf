namespace Calaf.Application

type BuildType =
    | Release
    | Nightly
    
type Command =
    | Build of BuildType