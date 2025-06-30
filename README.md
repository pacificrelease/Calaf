[![Build Solution](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml)

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

| Component part | Build            | Example      | Required |
|----------------|------------------|--------------|----------|
| Year           | YYYY             | 2025         | ✅       |
| Month          | MM               | 1, 12        | ✅       |
| Patch          | PATCH            | 1, 2, 999    | ❌       |
| Build          | BUILD.DAY.NUMBER | nightly.15.1 | ❌       |


YYYY - A year of the version. Always required.

MM - A month of the version. Always required.

PATCH - A patch number in the version's month.

BUILD.DAY.NUMBER - A type of the build with the day of the month, and number of the build in this day.

Currently available type of the build is: `nightly`

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