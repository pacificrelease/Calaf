namespace Calaf.Application

type SpaceApplyContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
    Console: IConsole
}