[![Build Solution](https://github.com/pacificrelease/Calaf/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/pacificrelease/Calaf/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Calaf.svg)](https://www.nuget.org/packages/Calaf/)
[![NuGet](https://img.shields.io/nuget/dt/Calaf.svg?color=black)](https://www.nuget.org/packages/Calaf/)
[![GitHub License](https://img.shields.io/badge/license-Apache%202-navy.svg)](https://raw.githubusercontent.com/pacificrelease/Calaf/main/LICENSE)

# Calaf

Calaf is a command-line tool for managing Calendar Versioning ([CalVer](https://calver.org)) of .NET projects, written in F#.

## Features

- Automatic versioning based on current date
- Support for stable and nightly builds
- Works with csproj/fsproj projects formats
- Tool installation via dotnet CLI

## Requirements

- .NET 8.0 or later

## Versioning Scheme

Calaf implements a Calendar Versioning ([CalVer](https://calver.org)) scheme that is compatible with Semantic Versioning 2.0.0. This ensures that versions are chronological, sortable, and widely supported.

**Format**: `YYYY.MM[.PATCH][-BUILD.FORMAT]`

### Core Components:

#### YYYY - Full year (required)

- Examples: `2001`, `2025`, `2150`
- Range: `1970` to `9999`

#### MM - Month number (required)

- Examples: `1`, `6`, `12`
- Range: `1` to `12`

#### PATCH - Patch number within the month (optional)

- Examples: `1`, `2`, `3`
- Range: `1` to `4294967295`

### Pre-release Components:

#### BUILD.FORMAT - Pre-release build identifier with a specific format suffix to indicate non-stable builds:

* **Beta releases:** `beta.NUMBER`

  * Example: `2025.6.1-beta.1`
  * Range `NUMBER`: `1` to `4294967295`

* **Nightly builds:** `0.nightly.DAY.NUMBER`

  * The leading `0` ensures that nightly builds have lower precedence than other pre-release builds like `beta`.
  * `DAY`: The day of the month
  * `NUMBER`: A sequential number for builds on the same day
  * Example: `2025.6.1-0.nightly.30.1`
  * Range `NUMBER`: `1` to `4294967295`

* **Beta nightly builds:** `beta.BETA_NUMBER.DAY.NIGHTLY_NUMBER`

  * Example: `2025.6.1-beta.1.30.1`

### Version Precedence

Versions are compared according to SemVer 2.0.0 rules. The following list shows an example of version progression from lowest to highest precedence:

1. `2025.6.1-0.nightly.30.1` (Nightly)
2. `2025.6.1-beta.1` (Beta)
3. `2025.6.1-beta.1.30.1` (Beta Nightly)
4. `2025.6.1-beta.2` (Later Beta)
5. `2025.6.1` (Stable Release)


## Installation

```bash
dotnet tool install -g Calaf
```


## Getting Started

### 1. Initialize Your Project Version

Add an initial Calendar Version to your project file(s) (`*.csproj` or `.fsproj`):

```xml
<PropertyGroup>
    <Version>2025.6</Version>
</PropertyGroup>
```

### 2. Create Versions

Calaf supports three release types: **`stable`**, **`beta`**, and **`nightly`**.

##### Stable Releases

Generate production-ready versions:

```bash
# Example (assuming today is June 30, 2025): 2025.6 → 2025.6.1
calaf make stable
```

Creates a stable Calendar Version using the **current UTC date** from running system (`System.DateTimeOffSet.UtcNow`). 

##### Beta Releases

Generate pre-release versions for testing:

```bash
# Example: 2025.6 → 2025.6.1-beta.1
calaf make beta 
```

Creates a beta version using the **current UTC date**. Beta numbering starts from `1` and increments with each new beta release. 

##### Nightly Builds

Generate development builds with daily identifiers:

```bash
# 2025.6 → 2025.6.1-0.nightly.30.1
calaf make nightly 
```

Creates a nightly build version using the **current date and day of the month**. Nightly numbering starts from `1` for each day.

**Note:** You can also create nightly builds from existing beta versions:

* `2025.6.1-beta.1` → `2025.6.1-beta.1.30.1`


## Further Use

The following example illustrates how to integrate Calaf into a CI/CD pipeline for automated Calendar Versioning:

```yaml
jobs:
    release:
        name: Release
        runs-on: ubuntu-latest
        steps:
            ...
            - name: Install Calaf
              run: dotnet tool install -g Calaf

            - name: Make Version
              run: |
                calaf make stable
              continue-on-error: false
            
            - name: Push Version
              run: |                
                git push origin ${{ github.ref_name }}
                git push origin --tags
            ...