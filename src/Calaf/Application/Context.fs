namespace Calaf.Application

type BumpContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
}