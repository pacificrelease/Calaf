[![Build](https://github.com/pacificrelease/Calaf/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/pacificrelease/Calaf/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Calaf.svg)](https://www.nuget.org/packages/Calaf/)
[![NuGet](https://img.shields.io/nuget/dt/Calaf.svg?color=black)](https://www.nuget.org/packages/Calaf/)
[![GitHub License](https://img.shields.io/badge/license-Apache%202-navy.svg)](https://raw.githubusercontent.com/pacificrelease/Calaf/main/LICENSE)

# Calaf

Calaf is a command-line tool for managing Calendar Versioning ([CalVer](https://calver.org)) of .NET projects, written in F#.


## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Versioning Scheme](#versioning-scheme)
- [Quick Start](#quick-start)
- [Commands Reference](#commands-reference)
- [Further Use](#further-use)
- [License](#license)


## Features

- Automatic Calendar Versioning based on the current UTC date
- Support for `stable`, `alpha`, `beta`, and `nightly` release types
- Git integration with automatic tagging (+version commit) for new versions
- Works with C#/F# project formats (`*.csproj`/`*.fsproj`)
- Generates versions compatible with Semantic Versioning 2.0.0
- Tool installation via dotnet CLI


## Requirements

- .NET 8.0 or later
- Git (required for automatic commit and tagging functionality when creating new versions)


## Versioning Scheme

Calaf implements a Calendar Versioning ([CalVer](https://calver.org)) scheme that is compatible with Semantic Versioning 2.0.0. This ensures that versions are chronological, sortable, and widely supported.

**Format**: `YYYY.MM[.PATCH][-BUILD.FORMAT]`

### Core Components:

#### YYYY - Full year (required)

- Examples: `2001`, `2025`, `2150`
- Range: `1` to `9999`

#### MM - Month number (required)

- Examples: `1`, `6`, `12`
- Range: `1` to `12`

#### PATCH - Patch number within the month (optional)

- Examples: `1`, `2`, `3`
- Range: `1` to `4294967295`

### Pre-release Components:

#### BUILD.FORMAT - Pre-release build identifier with a specific format suffix to indicate non-stable builds:

* **Alpha releases:** `alpha.NUMBER`

  * Example: `2025.8.1-alpha.1`
  * Range `NUMBER`: `1` to `4294967295`

* **Beta releases:** `beta.NUMBER`

  * Example: `2025.8.1-beta.1`
  * Range `NUMBER`: `1` to `4294967295`

* **Nightly builds:** `0.nightly.DAY.NUMBER`

  * The leading `0` ensures that nightly builds have lower precedence than other pre-release builds like `alpha`, `beta`.
  * `DAY`: The day of the month
  * `NUMBER`: A sequential number for builds on the same day
  * Example: `2025.7.1-0.nightly.30.1`
  * Range `NUMBER`: `1` to `4294967295`

* **Alpha nightly builds:** `alpha.ALPHA_NUMBER.DAY.NIGHTLY_NUMBER`

  * Example: `2025.8.1-alpha.1.30.1`

* **Beta nightly builds:** `beta.BETA_NUMBER.DAY.NIGHTLY_NUMBER`

  * Example: `2025.8.1-beta.1.30.1`

### Version Precedence

Versions are compared according to SemVer 2.0.0 rules. The following list shows an example of version progression from lowest to highest precedence:

1. `2025.8.1-0.nightly.30.1` (Nightly)
2. `2025.8.1-alpha.1` (Alpha)
3. `2025.8.1-alpha.1.30.1` (Alpha Nightly)
4. `2025.8.1-alpha.2` (Later Alpha)
5. `2025.8.1-beta.1` (Beta)
6. `2025.8.1-beta.1.30.1` (Beta Nightly)
7. `2025.8.1-beta.2` (Later Beta)
8. `2025.8.1` (Stable Release)


## Quick Start

##### 1. Installation:

```bash
dotnet tool install -g Calaf
```

##### 2. Add an initial version to your project file(s) (`*.csproj` or `*.fsproj`):

```xml
<PropertyGroup>
    <Version>2025.8</Version>
</PropertyGroup>
```

##### 3. Generate a stable Calendar Version:

```bash
calaf make stable
```


## Commands Reference

### `calaf make <type>`

**Synopsis:**
```console
calaf make <stable|alpha|beta|nightly>
```

**Description:** Generates and applies a new Calendar Version to all `.csproj` and `.fsproj` files in the current directory. Automatically creates Git commit and tag if repository is detected.

**Arguments:**

| Argument      | Description                                                                                                                                                               |
|---------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`stable`**  | Stable release. Creates a production-ready version.                                                                                                                       |
| **`alpha`**   | Alpha release. Creates an early pre-release version for testing. Increments the alpha number. Alpha numbering starts from `1` and increments with each new beta release.  |
| **`beta`**    | Beta release. Creates a pre-release version for testing. Increments the beta number. Beta numbering starts from `1` and increments with each new beta release.            |
| **`nightly`** | Nightly (development) build. Creates a development build version. Uses the current day and an incremental number that's starts from `1` for that day.                     |

**Examples:**

**Stable Release**

Creates a production version based on current UTC date on running system (`System.DateTimeOffSet.UtcNow`).
`2025.8` → `2025.8.1`

* Command:
```console
calaf make stable
```

* Output:
```console
Version applied: 2025.8.1
```

* Project file(s) change:
```xml
<!-- Before -->
<Version>2025.8</Version>

<!-- After -->
<Version>2025.8.1</Version>
```

**Alpha Release**

Creates an early pre-release version.
`2025.8.1` → `2025.8.1-alpha.1`

```console
calaf make alpha 
```

* Output:
```console
Version applied: 2025.8.1-alpha.1
```

* Project file(s) change:
```xml
<!-- Before -->
<Version>2025.8.1</Version>

<!-- After -->
<Version>2025.8.1-alpha.1</Version>
```

**Beta Release**

Creates a pre-release version.
`2025.8.1` → `2025.8.1-beta.1`

```console
calaf make beta 
```

* Output:
```console
Version applied: 2025.8.1-beta.1
```

* Project file(s) change:
```xml
<!-- Before -->
<Version>2025.8.1</Version>

<!-- After -->
<Version>2025.8.1-beta.1</Version>
```

**Nightly Build**

Creates a development build with daily identifier.
`2025.8.1` → `2025.8.2-0.nightly.27.1`

```console
calaf make nightly
```

* Output:
```console
Version applied: 2025.8.2-0.nightly.27.1
```

**Note:** Subsequent runs on the same day increment the build number: `2025.8.2-0.nightly.27.2`, `2025.8.2-0.nightly.27.3`, etc.

Project file change:
```xml
<!-- Before -->
<Version>2025.8.1</Version>

<!-- After -->
<Version>2025.8.2-0.nightly.27.1</Version>
```

**Note:** Nightly from Alpha

You can create nightly builds from existing alpha versions.
`2025.8.1-alpha.1` → `2025.8.1-alpha.1.27.1`

* Output:
```console
Version applied: 2025.8.1-alpha.1.27.1
```

Project file change:
```xml
<!-- Before -->
<Version>2025.8.1-alpha.1</Version>

<!-- After -->
<Version>2025.7.1-alpha.1.27.1</Version>
```

**Note:** Nightly from Beta

You can create nightly builds from existing beta versions.
`2025.8.1-beta.1` → `2025.8.1-beta.1.27.1`

* Output:
```console
Version applied: 2025.8.1-beta.1.27.1
```

Project file change:
```xml
<!-- Before -->
<Version>2025.8.1-beta.1</Version>

<!-- After -->
<Version>2025.8.1-beta.1.27.1</Version>
```


## Further Use

The following example illustrates how to integrate Calaf into a CI/CD pipeline for automated Calendar Versioning:

```yaml
jobs:
    release:
        name: Release
        runs-on: ubuntu-latest
        steps:
          - name: Checkout code
            uses: actions/checkout@v4
            with:
              # Fetch all history for accurate version calculation
              fetch-depth: 0
  
          - name: Install Calaf
            run: dotnet tool install -g Calaf
  
          - name: Make a new stable version
            id: versioning
            run: calaf make stable
            continue-on-error: false
            
          - name: Push version changes
            run: |
              git push origin ${{ github.ref_name }}
              git push origin --tags
```


## License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE) file for details.