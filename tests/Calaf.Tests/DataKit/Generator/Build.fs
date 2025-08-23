namespace Calaf.Tests.Generator

open FsCheck
open FsCheck.FSharp

open Calaf.Domain.DomainTypes.Values
open Calaf.Tests.Generator.Primitive

module Build =
    let private genBeforeUpperBoundaryUInt16 =
        Gen.choose(int 1, int (System.UInt16.MaxValue - 1us)) |> Gen.map uint16
        
    let private genAlphaPrefix =
        Gen.elements ["alpha"; "ALPHA"; "Alpha"; "AlPhA"; "aLpHa"; "ALPHA"; "alPHA"; "alpHA"; "AlpHA"; "ALpHA"; "ALPhA"; "aLPhA"; "aLpHA"; "aLPha"; "aLPHa"; "AlPha"; "AlpHa"; "aLpha"; "ALpha"; "AlphA"; "alphA"; "ALPHA"]
        
    let private genBetaPrefix =
        Gen.elements ["beta"; "BETA"; "Beta"; "BeTa"; "bEtA"; "bETA"; "beTA"; "betA"; "BEta"; "BETa"]
        
    let private genReleaseCandidatePrefix =
        Gen.elements ["rc"; "RC"; "Rc"; "rC"]
        
    let private genNightlyPrefix =
        Gen.elements ["nightly"; "NIGHTLY"; "Nightly"; "NiGhTlY"; "nIgHtLy"; "NIGHTly"; "nightLY"; "NightLy"; "nighTLY"; "NiGhTLy"; "nIGhTly"; "niGhtly"; "niGhTly"; "niGhTLy"; "nIghtly"; "NightLY"; "NIghtly"; "NigHtly"; "NightLy"; "NightlY"; "nIGHTLY"; "niGHTLY"; "niGhTLY"; "niGhTlY"; "niGHTly"; "niGHTLy"; "niGHTLY"; "nIGhTLY"; "nIGHTly"; "nIGHTLy"; "nIGHTLY"; "NIGhTLY"]
    
    let private genAlphaBuild : Gen<AlphaBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }
    
    let private genBetaBuild : Gen<BetaBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }
        
    let private genReleaseCandidateBuild : Gen<ReleaseCandidateBuild> =
        gen {
            let! number = genUInt16
            return { Number = number }
        }
        
    let private genNightlyBuild =
        gen {
            let! day = genDay
            let! number = genUInt16
            return { Day = day; Number = number } 
        }
    
    let private genAlphaNightlyBuild =
        gen {
            let! alphaBuild = genAlphaBuild
            let! nightlyBuild = genNightlyBuild
            return (alphaBuild, nightlyBuild)
        }
        
    let private genBetaNightlyBuild =
        gen {
            let! betaBuild = genBetaBuild
            let! nightlyBuild = genNightlyBuild
            return (betaBuild, nightlyBuild)
        }
        
    let private genReleaseCandidateNightlyBuild =
        gen {
            let! rcBuild = genReleaseCandidateBuild
            let! nightlyBuild = genNightlyBuild
            return (rcBuild, nightlyBuild)
        }
        
    let wrongString =
        let letterChars = ['a'..'z'] @ ['A'..'Z']
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        gen {
            let! shuffle = Gen.shuffle (letterChars @ specialChars)
            let! length = Gen.choose(64, 512)
            let! chars = Gen.arrayOfLength length (Gen.elements shuffle)
            return System.String(chars)
        }
        
    let wrongString2 =            
        let specialChars = ['!'; '@'; '#'; '$'; '%'; '^'; '&'; '*'; '('; ')'; '-'; '_'; '+'; '='; '~'; '?'; '/'; '\\'; '['; ']'; '{'; '}'; '|'; '<'; '>'; ','; '.'; ':']
        gen {
            let! length = Gen.choose(64, 512)
            let! chars = Gen.arrayOfLength length (Gen.elements specialChars)
            return System.String(chars)
        }
        
    let genContainingBadString (goodString: string) =
        gen {
            let! specialCharacters = wrongString2
            let! leadingWhiteSpaces = genWhiteSpacesString
            let! choice = Gen.frequency [
                1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{goodString}"
                1, Gen.constant $"{goodString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                1, Gen.constant $"{goodString}{wrongString}{goodString}"
                1, Gen.constant $"{specialCharacters}{goodString}{specialCharacters}"
                1, Gen.constant $"{specialCharacters}{goodString}"
                1, Gen.constant $"{goodString}{specialCharacters}"
                1, Gen.constant $"{goodString}{leadingWhiteSpaces}"
                1, Gen.constant $"{leadingWhiteSpaces}{goodString}"
                1, Gen.constant $"{leadingWhiteSpaces}{goodString}{leadingWhiteSpaces}"
                1, Gen.constant $"{goodString}{goodString}"
                1, Gen.constant $"{goodString}{goodString}{goodString}"                    
            ]
            return choice
        }
        
    module Alpha =
        let String =
            gen {
                let! alphaPrefix = genAlphaPrefix
                let! alpha = genAlphaBuild
                return $"{alphaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{alpha.Number}"
            }
            
        let BadString=
            gen {
                let! alphaString = String
                let! badString = genContainingBadString alphaString
                return badString
            }
            
        let Accidental =
            genAlphaBuild
            
        let AccidentalBuild =
            gen {
                let! a = genAlphaBuild
                return Build.Alpha a
            }
            
        let AccidentalBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, Accidental |> Gen.map Some
            ]
            
        let NoUpperBoundaryAccidentalBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Alpha { Number = number }                
            }
        
    module Beta =
        let betaString =
            gen {
                let! betaPrefix = genBetaPrefix
                let! beta = genBetaBuild
                return $"{betaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{beta.Number}"
            }
            
        let containingBetaBadString =
            gen {
                let! betaString = betaString
                let! badString = genContainingBadString betaString
                return badString
            }
            
        let beta =
            genBetaBuild
            
        let betaBuild =
            gen {
                let! b = genBetaBuild
                return Build.Beta b
            }
            
        let betaBuildOption =
            gen {
                let! choice = Gen.frequency [
                    1, Gen.constant None
                    3, betaBuild |> Gen.map Some
                ]
                return choice
            }
            
        let numberNoUpperBoundaryBetaBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Beta { Number = number }                
            }
            
    module ReleaseCandidate =
        let rcString =
            gen {
                let! rcPrefix = genReleaseCandidatePrefix
                let! rc = genReleaseCandidateBuild
                return $"{rcPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{rc.Number}"
            }
            
        let containingReleaseCandidateBadString =
            gen {
                let! betaString = rcString
                let! badString = genContainingBadString betaString
                return badString
            }
            
        let rc =
            genReleaseCandidateBuild
            
        let rcBuild =
            gen {
                let! rc = genReleaseCandidateBuild
                return Build.ReleaseCandidate rc
            }
            
        let rcBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, rcBuild |> Gen.map Some
            ]
            
        let numberNoUpperBoundaryReleaseCandidateBuild =
            gen {
                let! number = genBeforeUpperBoundaryUInt16
                return Build.ReleaseCandidate { Number = number }                
            }
            
    module Nightly =
        let nightlyString =
            gen {
                let! nightlyPrefix = genNightlyPrefix
                let! nightly = genNightlyBuild
                return $"{Calaf.Domain.Build.NightlyZeroPrefix}{Calaf.Domain.Build.NightlyZeroBuildTypeDivider}{nightlyPrefix}{Calaf.Domain.Build.BuildTypeDayDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            }
            
        let containingNightlyBadString =
            gen {
                let! nightlyString = nightlyString
                let! specialCharacters = wrongString2
                let! leadingWhiteSpaces = genWhiteSpacesString
                let! choice = Gen.frequency [
                    1, Gen.constant $"{Calaf.Domain.Build.BuildTypeDayDivider}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{Calaf.Domain.Build.BuildTypeDayDivider}"
                    1, Gen.constant $"{nightlyString}{wrongString}{nightlyString}"
                    1, Gen.constant $"{specialCharacters}{nightlyString}{specialCharacters}"
                    1, Gen.constant $"{specialCharacters}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{specialCharacters}"
                    1, Gen.constant $"{nightlyString}{leadingWhiteSpaces}"
                    1, Gen.constant $"{leadingWhiteSpaces}{nightlyString}"
                    1, Gen.constant $"{leadingWhiteSpaces}{nightlyString}{leadingWhiteSpaces}"
                    1, Gen.constant $"{nightlyString}{nightlyString}"
                    1, Gen.constant $"{nightlyString}{nightlyString}{nightlyString}"                    
                ]
                return choice
            }
        
        let nightly =
            genNightlyBuild
            
        let nightlyBuild =
            gen {
                let! n = genNightlyBuild
                return Build.Nightly n
            }
            
        let nightlyBuildOption =
            gen {
                let! choice = Gen.frequency [
                    1, Gen.constant None
                    3, nightlyBuild |> Gen.map Some
                ]
                return choice
            }
            
        let numberNoUpperBoundaryNightlyBuild =
            gen {
                let! day = genDay
                let! number = genBeforeUpperBoundaryUInt16
                return Build.Nightly { Day = day; Number = number }                
            }
    
    module AlphaNightly =
        let String =
            gen {
                let! alphaPrefix = genAlphaPrefix
                let! alpha, nightly = genAlphaNightlyBuild
                return $"{alphaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{alpha.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let BadString =
            gen {
                let! alphaNightlyString = String
                let! badString = genContainingBadString alphaNightlyString
                return badString
            }
            
        let AccidentalBuild =
            gen {
                let! an = genAlphaNightlyBuild
                return Build.AlphaNightly an
            }
            
        let AccidentalBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, AccidentalBuild |> Gen.map Some
            ]
        
        let NoUpperBoundaryAccidentalBuild =
            gen {
                let! alphaNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.AlphaNightly({ Number = alphaNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }
            
    module BetaNightly =
        let betaNightlyString =
            gen {
                let! betaPrefix = genBetaPrefix
                let! beta, nightly = genBetaNightlyBuild
                return $"{betaPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{beta.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let containingBetaNightlyBadString =
            gen {
                let! betaNightlyString = betaNightlyString
                let! badString = genContainingBadString betaNightlyString
                return badString
            }
            
        let betaNightlyBuild =
            gen {
                let! bn = genBetaNightlyBuild
                return Build.BetaNightly bn
            }
            
        let betaNightlyBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, betaNightlyBuild |> Gen.map Some
            ]
        
        let numberNoUpperBoundaryBetaNightlyBuild =
            gen {
                let! betaNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.BetaNightly({ Number = betaNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }
            
    module ReleaseCandidateNightly =
        let rcNightlyString =
            gen {
                let! rcPrefix = genReleaseCandidatePrefix
                let! rc, nightly = genReleaseCandidateNightlyBuild
                return $"{rcPrefix}{Calaf.Domain.Build.BuildTypeNumberDivider}{rc.Number}{Calaf.Domain.Build.PreReleaseNightlyDivider}{nightly.Day}{Calaf.Domain.Build.DayNumberDivider}{nightly.Number}"
            } 
        
        let containingReleaseCandidateNightlyBadString =
            gen {
                let! rcNightlyString = rcNightlyString
                let! badString = genContainingBadString rcNightlyString
                return badString
            }
            
        let rcNightlyBuild =
            gen {
                let! rcn = genReleaseCandidateNightlyBuild
                return Build.ReleaseCandidateNightly rcn
            }
            
        let rcNightlyBuildOption =
            Gen.frequency [
                1, Gen.constant None
                3, rcNightlyBuild |> Gen.map Some
            ]
        
        let numberNoUpperBoundaryReleaseCandidateNightlyBuild =
            gen {
                let! rcNumber = genBeforeUpperBoundaryUInt16
                let! nightlyDay = genDay
                let! nightlyNumber = genBeforeUpperBoundaryUInt16
                return Build.ReleaseCandidateNightly({ Number = rcNumber }, { Day = nightlyDay; Number = nightlyNumber })                
            }