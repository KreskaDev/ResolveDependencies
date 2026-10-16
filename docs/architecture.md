---
title: Architecture
description: Structure of the solution, dependency direction between projects and the responsibility of each project.
related:
  - docs/tech_design.md
---
# Architecture

## Goals and constraints

- Keep the core logic independent of any I/O so it can be unit-tested in isolation.
- Keep the solution small.

## Technology

| Concern           | Choice                                                              |
|-------------------|---------------------------------------------------------------------|
| Platform          | .NET 10 (`net10.0`), C#                                             |
| Test framework    | xUnit                                                               |
| Language features | `Nullable` and `ImplicitUsings` enabled; warnings treated as errors |

## Projects and dependency direction

```
ResolveDependencies.Console ──► ResolveDependencies.Core ◄── ResolveDependencies.Tests
```

- **Core** has no project references and no package references. It knows nothing about
  files, folders, the console or the text format. Its public API accepts a structurally
  valid `InstallationRequest` and returns a `ValidationResult`; whether the installation
  is valid is the only question Core answers.
- **Console** references Core. It adapts the file system and the command line to Core:
  enumerates the input files, sanitizes and parses their text into an
  `InstallationRequest`, hands it to Core, writes the output files and the console
  summary. It contains no domain logic.
- **Tests** reference Core and Console. Core is covered through its domain services,
  Console through its command line parser, sanitizer, parser, file processor and report.
  Only `Program` has no unit test; file enumeration and output writing are checked by
  running the application against `docs/story/goldenData/`.

## Folder layout

Each project is split into folders by responsibility; the namespace of a type equals its
folder path. Entry points stay at the project root.

```
ResolveDependencies.Core/
  Model/          PackageVersion, Dependency, InstallationRequest   -- the contract Core receives
  Graph/          DependencyGraph, DependencyGraphBuilder           -- adjacency list of the rules
  Validation/     InstallationValidator, ValidationResult, Conflict -- semantic rules and their outcome
  DependencyResolver                                                -- composes graph and validation

ResolveDependencies.Console/
  CommandLine/    CommandLineParser, CommandLineOptions, CommandLineException
  Input/          InputSanitizer, InputParser, InputFormatException  -- text to InstallationRequest
  Processing/     InputFileProcessor, FileReport, Verdict            -- one file in, one report out
  Program                                                            -- process, file system, exit code

ResolveDependencies.Tests/
  Core/           tests of the Core services
  Console/        tests of the Console components
  InstallationRequestBuilder                                         -- shared test helper
```

## Console application

- Responsibilities: arguments, reading and sanitizing the input files, parsing them into
  the Core contract, writing the output files and the console summary, exit codes.
- No dependency-injection container or logging framework; the app is small enough that a
  plain `Main` is clearer.
