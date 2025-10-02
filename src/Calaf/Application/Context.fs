namespace Calaf.Application

type MakeContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
    Console: IConsole
}