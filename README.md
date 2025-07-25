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

1. Add init version to your projects files:

```xml
<PropertyGroup>
    <Version>2025.6</Version>
</PropertyGroup>
```

2. Manage project versioning using Calaf:

The supported releases: `stable`, `beta`, `nightly`.

```bash
# Create a stable version (e.g., 2025.6 → 2025.6.1)
calaf make stable
```

Updates the project version to a stable Calendar Version based on the current UTC date.

```bash
# Create a beta build (e.g., 2025.6 → 2025.6.1-beta.1)
calaf make beta 
```

Updates the project version to a beta build version based on the current UTC date. `beta` initial Number is `1`

```bash
# Let's imagine that today's date it: 30 of June 2025:
# Create a nightly build (e.g., 2025.6 → 2025.6.1-nightly.30.1)
# Beta build can have a nightly suffix too (e.g., 2025.6.1-nightly.30.1 -> 2025.6.1-beta.1.30.2)
calaf make nightly 
```

Updates the project version to a nightly build version based on the current date and day of the month.

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