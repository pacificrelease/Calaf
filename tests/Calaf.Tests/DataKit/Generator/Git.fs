module Calaf.Tests.Generator.Git

open FsCheck
open FsCheck.FSharp

open Calaf.Contracts
open Calaf.Domain.DomainTypes.Values
open Calaf.Domain.DomainTypes.Entities

open Calaf.Tests.Generator.Primitive
    
let branchName =            
    Gen.frequency [ 1, SematicVersion.semanticVersionTagStr
                    1, Gen.elements [ "master"; "main"; "develop"; "feature"; "bugfix"; "release" ]]            

let branchNameOrNone =
    Gen.frequency [ 1, Gen.constant None
                    3, branchName |> Gen.map Some ]
    
    
let commitText =
    Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                    1, Gen.elements [""; " "]]

let commitHash =
    gen {
        return Bogus.Faker().Random.Hash()
    }
    
module Commit =
    let private leftBracketOrEmpty scope =
        if System.String.IsNullOrWhiteSpace scope
        then System.String.Empty
        else "("
        
    let private rightBracketOrEmpty scope =
        if System.String.IsNullOrWhiteSpace scope
        then System.String.Empty
        else ")"
        
    let private otherCommitMessage =
        gen {
            let! text =
                Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                                1, Gen.elements [""; " "]]
            if System.String.IsNullOrWhiteSpace text
            then return (text, CommitMessage.Other None)
            else return (text, CommitMessage.Other (Some text))
        }
        
    let private fixNonBreakingChangeCommitMessage =
        gen {
            let! desc =
                Gen.frequency [ 3, Gen.constant (Bogus.Faker().Lorem.Sentence())
                                1, Gen.elements [""; " "]]
            let! scope =
                Gen.frequency [ 3, Gen.map (fun word -> $"{word}") (Gen.constant (Bogus.Faker().Lorem.Word()))
                                1, Gen.elements [""; " "]]
            let text = $"{Calaf.Domain.Commit.FixPrefix}{leftBracketOrEmpty scope}{scope}{rightBracketOrEmpty scope}{Calaf.Domain.Commit.EndOfPattern} {desc}"
            let desc =
                if System.String.IsNullOrWhiteSpace desc
                then None
                else Some desc
            let scope =
                if System.String.IsNullOrWhiteSpace scope
                then None
                else Some scope
            
            return (text, CommitMessage.Fix (false, scope, desc))
        }
        
    let otherCommit: Gen<Commit> =
        gen {
            let! text, message = otherCommitMessage
            let! commitHash = commitHash
            let! timeStamp = genValidDateTimeOffset
            return { Message = message
                     Text    = text
                     Hash    = commitHash
                     When    = timeStamp }
        }
        
    let fixNonBreakingChangeCommit: Gen<Commit> =
        gen {
            let! text, message = fixNonBreakingChangeCommitMessage
            let! commitHash = commitHash
            let! timeStamp = genValidDateTimeOffset
            return { Message = message
                     Text    = text
                     Hash    = commitHash
                     When    = timeStamp }
        }
    
let gitCommitInfo : Gen<GitCommitInfo> =
    gen {
        let! commitText = commitText
        let! commitHash = commitHash
        let! timeStamp = genValidDateTimeOffset
        return { Text = commitText; Hash = commitHash; When = timeStamp }
    }
    
let gitSignatureInfo : Gen<GitSignatureInfo> =
    gen {
        let! email = Gen.constant (Bogus.Faker().Person.Email)
        let! name = Gen.constant (Bogus.Faker().Person.FullName)
        let! timeStamp = Gen.constant (Bogus.Faker().Date.BetweenOffset(System.DateTimeOffset.MinValue, System.DateTimeOffset.MaxValue))
        return { Email = email; Name = name; When = timeStamp }
    }
    
let gitTagInfo =
    gen {
        let! commit = gitCommitInfo
        let name    = commit.Text 
        let! commit = Gen.frequency [
            1, Gen.constant None
            3, Gen.constant (Some commit)
        ]
        return { Name = name; Commit = commit }
    }        
        
let calVerGitTagInfo =
    gen {
        let! validCalVerString = CalendarVersion.TagString
        let! maybeCommit = Gen.frequency [
            1, Gen.constant None
            3, gitCommitInfo |> Gen.map Some
        ]
        return { Name = validCalVerString; Commit = maybeCommit }                                
    }
    
let semVerGitTagInfo =
    gen {
        let! validSemVerString = SematicVersion.semanticVersionTagStr
        let! maybeCommit = Gen.frequency [
            1, Gen.constant None
            3, gitCommitInfo |> Gen.map Some
        ]
        return { Name = validSemVerString; Commit = maybeCommit }                       
    }
    
let unversionedTagName =
    Gen.frequency [
            3, nonNumericString
            2, invalidThreePartString
            1, nullOrWhiteSpaceString
        ]
    
let malformedGitTagInfo =
    gen {
        let! unversionedTagName = unversionedTagName
        let! maybeCommit = Gen.frequency [
            1, Gen.constant None
            3, gitCommitInfo |> Gen.map Some
        ]
        return { Name = unversionedTagName; Commit = maybeCommit }
    }
    
let calendarVersionOrSemanticVersionGitTagInfo =
    gen {
        let! tagName = Gen.frequency [
            3, CalendarVersion.TagString
            1, SematicVersion.semanticVersionTagStr
        ]
        let! commit = gitCommitInfo |> Gen.map Some
        return { Name = tagName; Commit = commit }
    }
    
let randomGitTagInfo =
    gen {
        let! choice = Gen.frequency [
            1, malformedGitTagInfo
            3, calVerGitTagInfo
            2, semVerGitTagInfo
        ]
        return choice
    }
    
let commitOrNone =
    Gen.frequency [
        1, Gen.constant None
        2, Commit.otherCommit |> Gen.map Some
        2, Commit.fixNonBreakingChangeCommit |> Gen.map Some
    ]
    
let calendarVersionTag : Gen<Tag> =            
    gen {
        // TODO: Rewrite generator!
        let! calendarVersion = CalendarVersion.Accidental
        let tagNameSb = System.Text.StringBuilder($"{calendarVersion.Year}.{calendarVersion.Month}")
        let tagNameSb = if calendarVersion.Micro.IsSome then tagNameSb.Append calendarVersion.Micro.Value else tagNameSb 
        let! maybeCommit = commitOrNone
        return Tag.Versioned (tagNameSb.ToString(), (calendarVersion |> CalVer), maybeCommit)
    }
    
let sematicVersionTag : Gen<Tag> =            
    gen {
        let! semanticVersion, stringEquivalent = SematicVersion.semanticVersion2
        let! maybeCommit = commitOrNone
        return Tag.Versioned (stringEquivalent, (SemVer semanticVersion), maybeCommit)
    }
    
let unversionedTag : Gen<Tag> =            
    gen {
        let! tagName = unversionedTagName
        return Tag.Unversioned tagName
    }
    
let calendarVersionsTagsArray =
     gen {
        let! smallCount = Gen.choose(1, 50)
        let! middleCount = Gen.choose(51, 100)            
        let! bigCount = Gen.choose(101, 250)            
        let! choice = Gen.frequency [
            7, Gen.arrayOfLength smallCount  calendarVersionTag
            2, Gen.arrayOfLength middleCount calendarVersionTag
            1, Gen.arrayOfLength bigCount    calendarVersionTag
        ]
        return choice
    }
     
let semanticVersionsTagsArray =
    gen {
        let! smallCount  = Gen.choose(1, 50)
        let! middleCount = Gen.choose(51, 100)            
        let! bigCount    = Gen.choose(101, 250)            
        let! choice = Gen.frequency [
            7, Gen.arrayOfLength smallCount  sematicVersionTag
            2, Gen.arrayOfLength middleCount sematicVersionTag
            1, Gen.arrayOfLength bigCount    sematicVersionTag
        ]
        return choice
    }
    
let unversionedTagsArray =
    gen {
        let! smallCount  = Gen.choose(1, 50)
        let! middleCount = Gen.choose(51, 100)            
        let! bigCount    = Gen.choose(101, 250)            
        return! Gen.frequency [
            7, Gen.arrayOfLength smallCount  unversionedTag
            2, Gen.arrayOfLength middleCount unversionedTag
            1, Gen.arrayOfLength bigCount    unversionedTag
        ]
    }
    
let semanticVersionsAndUnversionedTagsArray =
    gen {
        let! semanticVersionsTags = semanticVersionsTagsArray
        let! unversionedTags = unversionedTagsArray
        return Array.append semanticVersionsTags unversionedTags                
    }
    
let baseGitRepositoryInfo =
    gen {
        let! dir = directoryPathString
        let! unborn = genBool
        let! detached = genBool
        let! branch = branchNameOrNone
        let! commit = Gen.frequency [ 1, Gen.constant None; 3, gitCommitInfo |> Gen.map Some ]
        let! signature = Gen.frequency [1, Gen.constant None; 3, gitSignatureInfo |> Gen.map Some ]
        let! dirty = genBool
        let! tags = Gen.listOf gitTagInfo
        return {
            Directory = dir
            Unborn = unborn
            Detached = detached
            CurrentBranch = branch
            CurrentCommit = commit
            Signature = signature
            Dirty = dirty
            Tags = tags
        }
}