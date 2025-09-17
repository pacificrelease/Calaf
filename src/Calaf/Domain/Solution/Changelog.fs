module internal Calaf.Domain.Changelog

open Calaf.Contracts
open Calaf.Domain.DomainTypes

let tryCapture (fileInfo: FileInfo) : Changelog =
    { Metadata = {
        Name = fileInfo.Name
        Extension = fileInfo.Extension
        Directory = fileInfo.Directory
        AbsolutePath = fileInfo.AbsolutePath }
      FileExists = fileInfo.Exists }