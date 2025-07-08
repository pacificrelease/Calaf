namespace Calaf.Application

open Calaf.Contracts

// TODO: Remove this type after refactoring
type MakeContext = {
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
    Console: IConsole
}

type MakeContext2 = {
    Directory: string
    Type: MakeType
    Settings: MakeSettings
    FileSystem: IFileSystem
    Git: IGit
    Clock: IClock
}