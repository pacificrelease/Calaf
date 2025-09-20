## 2025.9

**20.09.2025**

### Features

-  add automatic CHANGELOG.md generating in the project root according to the Conventional Commits
-  add toString Changelog function, and change Changelog data types and type structure
-  add tryGetChangelog Make function to create changelog
-  add toString CommitMessage function to synthesise string representation of the commit message with tests
-  add tryCreate Changelog function
-  add Message property to Commit domain type
-  add adopted commit message and creation function to Commit module to use it in the future for the changelog creation
-  add append function to append string text to markdown file
-  add tryReadMarkdown to FileSystem to read existing markdown file
-  add tryListCommits function basics to IGit
-  introduce Micro versioning instead of Patch, and update related components

### Fixed

- **git**:  add timeout handling and UTF-8 support in Git process execution Close #9
- **git**:  correct porcelain token parsing logic and improve token handling in changes
- **git**:  correct whitespace handling & improve porcelain parsing in changes function
-  correct feature, fix string patterns, and add more data to ConventionalCommitMessage for the right string synthesis
-  correct CommitMessage patterns to pass full descriptions
-  correct CommitMessage creation and add tests
-  correct asDateTimeOffset internal function from the test's datakit
-  correct repository status reading for unborn repositories

