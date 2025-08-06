namespace Calaf.Tests.MakeTests

open System.Xml.Linq
open Xunit
open Swensen.Unquote

open Calaf.Application
open Calaf.Contracts

module Run2Tests =
    
    // Test data setup
    module TestData =
        let createTestProjectXml (version: string) =
            XElement(XName.Get("Project"),
                XElement(XName.Get("PropertyGroup"),
                    XElement(XName.Get("Version"), version)))
                    
        let createTestDirectory () =
            {
                Directory = "/test/workspace"
                Projects = [
                    {
                        Name = "TestProject"
                        Directory = "/test/workspace"
                        Extension = ".csproj"
                        AbsolutePath = "/test/workspace/TestProject.csproj"
                        Content = createTestProjectXml "2025.7"  // Use calendar version format
                    }
                ]
            }
            
        let createTestGitRepository () =
            Some {
                Directory = "/test/workspace"
                Unborn = false
                Detached = false
                CurrentBranch = Some "main"
                CurrentCommit = Some {
                    Hash = "abc123"
                    Message = "Initial commit"
                    When = System.DateTimeOffset.Now
                }
                Signature = Some {
                    Email = "test@example.com"
                    Name = "Test User"
                    When = System.DateTimeOffset.Now
                }
                Dirty = false
                Tags = [
                    {
                        Name = "v2025.7"  // Use calendar version format
                        Commit = Some {
                            Hash = "abc123"
                            Message = "Release v2025.7"
                            When = System.DateTimeOffset.Now
                        }
                    }
                ]
            }
            
        let createTestSettings () =
            let projectPattern = MakeSettings.tryCreateDotNetXmlFilePattern "*.csproj"
            let tagCount = MakeSettings.tryCreateTagCount 10uy
            match projectPattern, tagCount with
            | Ok pattern, Ok count -> 
                {
                    ProjectsSearchPattern = pattern
                    TagsToLoad = count
                }
            | _ -> failwith "Failed to create test settings"
            
    // Mock implementations
    module Mocks =
        type MockFileSystem(directoryResult: Result<DirectoryInfo, CalafError>, 
                           readXmlResults: Map<string, Result<XElement, CalafError>>,
                           writeXmlResults: Map<string, Result<unit, CalafError>>) =
            let mutable writtenFiles = []
            
            interface IFileSystem with
                member _.tryReadDirectory directory pattern =
                    directoryResult
                    
                member _.tryReadXml absolutePath =
                    readXmlResults 
                    |> Map.tryFind absolutePath 
                    |> Option.defaultValue (Error (CalafError.Infrastructure (InfrastructureError.FileSystem FileSystemError.DirectoryDoesNotExist)))
                    
                member _.tryWriteXml (absolutePath, content) =
                    writtenFiles <- (absolutePath, content) :: writtenFiles
                    writeXmlResults 
                    |> Map.tryFind absolutePath 
                    |> Option.defaultValue (Ok ())
                    
            member _.GetWrittenFiles() = List.rev writtenFiles
            
        type MockGit(repositoryResult: Result<GitRepositoryInfo option, CalafError>,
                    applyResults: Map<string, Result<unit, CalafError>>) =
            let mutable appliedOperations = []
            
            interface IGit with
                member _.tryRead directory maxTagsToRead tagsPrefixesToFilter timeStamp =
                    repositoryResult
                    
                member _.tryApply (directory, files) commitMessage tagName =
                    appliedOperations <- (directory, files, commitMessage, tagName) :: appliedOperations
                    applyResults 
                    |> Map.tryFind directory 
                    |> Option.defaultValue (Ok ())
                    
            member _.GetAppliedOperations() = List.rev appliedOperations
            
        type StubClock(fixedTime: System.DateTimeOffset) =
            interface IClock with
                member _.utcNow() =
                    fixedTime

    [<Fact>]
    let ``run2 with Stable type should execute stable workflow successfully`` () =
        // Arrange
        let testTime = System.DateTimeOffset(2025, 7, 8, 10, 30, 0, System.TimeSpan.Zero)
        let testDirectory = TestData.createTestDirectory()
        let testRepo = TestData.createTestGitRepository()
        let testSettings = TestData.createTestSettings()
        
        let mockFileSystem = Mocks.MockFileSystem(
            Ok testDirectory,
            Map.empty,
            Map.ofList [("/test/workspace/TestProject.csproj", Ok ())]
        )
        
        let mockGit = Mocks.MockGit(
            Ok testRepo,
            Map.ofList [("/test/workspace", Ok ())]
        )
        
        let mockClock = Mocks.StubClock(testTime)
        
        let context = {
            Directory = "/test/workspace"
            Type = MakeType.Stable
            Settings = testSettings
            FileSystem = mockFileSystem
            Git = mockGit
            Clock = mockClock
        }
        
        // Act
        let result = Make.run2 context
        
        // Assert
        test <@ Result.isOk result @>
        
        let writtenFiles = mockFileSystem.GetWrittenFiles()
        test <@ List.length writtenFiles = 1 @>
        
        let appliedOps = mockGit.GetAppliedOperations()
        test <@ List.length appliedOps = 1 @>
        
        let directory, files, commitMessage, tagName = List.head appliedOps
        test <@ directory = "/test/workspace" @>

    [<Fact>]
    let ``run2 with Nightly type should execute nightly workflow successfully`` () =
        // Arrange
        let testTime = System.DateTimeOffset(2025, 7, 8, 10, 30, 0, System.TimeSpan.Zero)
        let testDirectory = TestData.createTestDirectory()
        let testRepo = TestData.createTestGitRepository()
        let testSettings = TestData.createTestSettings()
        
        let mockFileSystem = Mocks.MockFileSystem(
            Ok testDirectory,
            Map.empty,
            Map.ofList [("/test/workspace/TestProject.csproj", Ok ())]
        )
        
        let mockGit = Mocks.MockGit(
            Ok testRepo,
            Map.ofList [("/test/workspace", Ok ())]
        )
        
        let mockClock = Mocks.StubClock(testTime)
        
        let context = {
            Directory = "/test/workspace"
            Type = MakeType.Nightly
            Settings = testSettings
            FileSystem = mockFileSystem
            Git = mockGit
            Clock = mockClock
        }
        
        // Act
        let result = Make.run2 context
        
        // Assert
        test <@ Result.isOk result @>
        
        let writtenFiles = mockFileSystem.GetWrittenFiles()
        test <@ List.length writtenFiles = 1 @>
        
        let appliedOps = mockGit.GetAppliedOperations()
        test <@ List.length appliedOps = 1 @>

    [<Fact>]
    let ``run2 should handle FileSystem errors gracefully`` () =
        // Arrange
        let testTime = System.DateTimeOffset(2025, 7, 8, 10, 30, 0, System.TimeSpan.Zero)
        let testSettings = TestData.createTestSettings()
        
        let fileSystemError = CalafError.Infrastructure (InfrastructureError.FileSystem FileSystemError.DirectoryDoesNotExist)
        let mockFileSystem = Mocks.MockFileSystem(
            Error fileSystemError,
            Map.empty,
            Map.empty
        )
        
        let mockGit = Mocks.MockGit(Ok None, Map.empty)
        let mockClock = Mocks.StubClock(testTime)
        
        let context = {
            Directory = "/invalid/path"
            Type = MakeType.Stable
            Settings = testSettings
            FileSystem = mockFileSystem
            Git = mockGit
            Clock = mockClock
        }
        
        // Act
        let result = Make.run2 context
        
        // Assert
        test <@ Result.isError result @>
        match result with
        | Error error -> test <@ error = fileSystemError @>
        | Ok _ -> failwith "Expected error result"

    [<Fact>]
    let ``run2 should handle Git errors gracefully`` () =
        // Arrange
        let testTime = System.DateTimeOffset(2025, 7, 8, 10, 30, 0, System.TimeSpan.Zero)
        let testDirectory = TestData.createTestDirectory()
        let testSettings = TestData.createTestSettings()
        
        let gitError = CalafError.Infrastructure (InfrastructureError.Git GitError.RepoNotInitialized)
        let mockFileSystem = Mocks.MockFileSystem(
            Ok testDirectory,
            Map.empty,
            Map.empty
        )
        
        let mockGit = Mocks.MockGit(
            Error gitError,
            Map.empty
        )
        
        let mockClock = Mocks.StubClock(testTime)
        
        let context = {
            Directory = "/test/workspace"
            Type = MakeType.Nightly
            Settings = testSettings
            FileSystem = mockFileSystem
            Git = mockGit
            Clock = mockClock
        }
        
        // Act
        let result = Make.run2 context
        
        // Assert
        test <@ Result.isError result @>
        match result with
        | Error error -> test <@ error = gitError @>
        | Ok _ -> failwith "Expected error result"

    [<Fact>]
    let ``run2 should work without Git repository (None case)`` () =
        // Arrange
        let testTime = System.DateTimeOffset(2025, 7, 8, 10, 30, 0, System.TimeSpan.Zero)
        let testDirectory = TestData.createTestDirectory()
        let testSettings = TestData.createTestSettings()
        
        let mockFileSystem = Mocks.MockFileSystem(
            Ok testDirectory,
            Map.empty,
            Map.ofList [("/test/workspace/TestProject.csproj", Ok ())]
        )
        
        let mockGit = Mocks.MockGit(
            Ok None, // No git repository
            Map.empty
        )
        
        let mockClock = Mocks.StubClock(testTime)
        
        let context = {
            Directory = "/test/workspace"
            Type = MakeType.Stable
            Settings = testSettings
            FileSystem = mockFileSystem
            Git = mockGit
            Clock = mockClock
        }
        
        // Act
        let result = Make.run2 context
        
        // Assert
        test <@ Result.isOk result @>
        
        let writtenFiles = mockFileSystem.GetWrittenFiles()
        test <@ List.length writtenFiles = 1 @>
        
        // Should not attempt git operations when no repo
        let appliedOps = mockGit.GetAppliedOperations()
        test <@ List.isEmpty appliedOps @>

    [<Fact>]
    let ``run2 should handle valid timestamps correctly for different years`` () =
        let testCases = [
            System.DateTimeOffset(2020, 6, 15, 14, 30, 0, System.TimeSpan.Zero)
            System.DateTimeOffset(2025, 12, 31, 23, 59, 59, System.TimeSpan.Zero)
            System.DateTimeOffset(2030, 1, 1, 0, 0, 0, System.TimeSpan.Zero)
        ]
        
        for timestamp in testCases do
            // Arrange
            let testDirectory = TestData.createTestDirectory()
            let testRepo = TestData.createTestGitRepository()
            let testSettings = TestData.createTestSettings()
            
            let mockFileSystem = Mocks.MockFileSystem(
                Ok testDirectory,
                Map.empty,
                Map.ofList [("/test/workspace/TestProject.csproj", Ok ())]
            )
            
            let mockGit = Mocks.MockGit(
                Ok testRepo,
                Map.ofList [("/test/workspace", Ok ())]
            )
            
            let mockClock = Mocks.StubClock(timestamp)
            
            let context = {
                Directory = "/test/workspace"
                Type = MakeType.Stable
                Settings = testSettings
                FileSystem = mockFileSystem
                Git = mockGit
                Clock = mockClock
            }
            
            // Act & Assert
            let result = Make.run2 context
            test <@ Result.isOk result @>
