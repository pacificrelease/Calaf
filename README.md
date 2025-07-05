[![Build Solution](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Calaf.svg)](https://www.nuget.org/packages/Calaf/)

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

The current supported scheme has a format:

YYYY.MM[.PATCH][.BUILD.DAY.NUMBER]


Where:

YYYY - A full year of the version, always required - e.g., `2001`, `2025`, `2150`. Values range: `1970` to `9999`.

MM - A short month of the version, always required - e.g., `1`, `6`, `12`. Values range: `1` to `12`.

PATCH - A patch number in the version's month optional - e.g., `1`, `2`, `3`. Values range: `1` to`4294967295`.

BUILD.DAY.NUMBER - A type of the build with the day of the month, and number of the build in this day, optional - e.g., `nightly.2.1`, `nightly.31.2`. Values range: `nightly` for nightly builds, and `1` to `99999` for the number of builds in a day.

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

```bash
# Create stable version (e.g., 2025.6 → 2025.6.1)
calaf make stable
```

Updates the project version to a stable Calendar Version based on the current UTC date.

```bash
# Create nightly build (e.g., 2025.6 → 2025.6.1-nightly.30.1)
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