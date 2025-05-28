namespace Calaf.Application

type BumpContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
}

type OutputContext = {
    Console: IConsole
}