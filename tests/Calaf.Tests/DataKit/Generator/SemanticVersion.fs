namespace Calaf.Tests.Generator

open FsCheck.FSharp

open Calaf.Domain.DomainTypes.Values
open Calaf.Tests.Generator.Primitive

module SematicVersion =
        let semanticVersion =
            let genBigSemVer =
                gen {
                    let! major = Gen.choose64(1000, int64 System.UInt32.MaxValue)
                    let! minor = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                    let! patch = Gen.choose64(100_000, int64 System.UInt32.MaxValue)
                    return uint32 major, uint32 minor, uint32 patch
                }
            let genSmallSemVer =
                gen {
                    let! major = Gen.choose(0, 999)
                    let! minor = Gen.choose(13, 99999)
                    let! patch = Gen.choose(1, 99999)
                    return uint32 major, uint32 minor, uint32 patch
                }
            gen {
                let! major, minor, patch = Gen.frequency [
                    1, genBigSemVer
                    1, genSmallSemVer
                ]
                return { Major = major; Minor = minor; Patch = patch }
            }
            
        let semanticVersion2 =
            gen {
                let! semanticVersion = semanticVersion
                return semanticVersion, $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}"
            }
            
        let semanticVersionStr =
            gen {
                let! semanticVersion = semanticVersion
                return $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}"
            }
            
        let semanticVersionTagStr =
            gen {
                let! validPrefix = genTagVersionPrefix
                let! semVer = semanticVersionStr
                return $"{validPrefix}{semVer}"
            }