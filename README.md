[![Build Solution](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mikhailovdv/Calaf/actions/workflows/build.yml)

# Calaf

Calaf is a command-line tool for managing the Calendar Versioning (https://calver.org) of .NET projects, written in F#.

## Supported Scheme

The current supported scheme has a format:

| Stable part     | Build part          |
|-----------------|---------------------|
| YYYY.MM(.PATCH) | (-BUILD.DAY.NUMBER) |

YYYY - A year of the version. Essential.

MM - A month of the version. Essential.

PATCH - A patch number in the version's month.

BUILD.DAY.NUMBER - A type of the build with the day of the month, and number of the build in this day.

Currently available type of the build is: `nightly`

## Getting Started

To get started, please initialize your projects first by adding the version tag to your projects.

```xml
<PropertyGroup>
    <Version>2025.1</Version>
</PropertyGroup>
```

then, install the Calaf:

```bash
dotnet tool install -g Calaf 
```

Done. You can easily bump the version by the command:

## Commands

```bash
calaf make stable 
```

Creates a stable Calendar Version according to the running environment UTC date and time.

```bash
calaf make nightly 
```

Creates a nightly build of the project. This command will update the version of the project to a nightly version, which is based on the current date.