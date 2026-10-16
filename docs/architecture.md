---
title: Architecture
description: Structure of the solution, dependency direction between projects, core components, validation algorithm and testing strategy.
related:
  - docs/tech_design.md
  - docs/test_design.md
  - docs/story/ResolveDependencies.md
---
# Architecture

This document covers only architecture.

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
  files, folders or the console. Its public API accepts text (`string` / `TextReader`) and
  returns plain result types.
- **Console** references Core. It is responsible for argument handling, enumerating input
  files in a folder, reading them, calling Core and writing results to standard output.
  It contains no domain logic.
- **Tests** reference Core only. The console project is a thin adapter and is covered by
  running it against `docs/story/goldenData/` rather than by unit tests.

## Console application

- Single responsibility: adapt the file system and the CLI to Core.
- Input: path to a folder. Processes every `*.txt` file whose name starts with `input`,
  sorted by name.
- Output: for every `inputNNN.txt` an `outputNNN.txt` containing `PASS`, `FAIL` or `ERROR`
  with the reason on the second line. Written next to the input file, or into the folder
  given by `--output <folder>`. One summary line per file goes to standard output.
- Exit code `0` when all files were processed, including files that produced `ERROR`;
  non-zero on invalid arguments or I/O errors.
- No dependency-injection container or logging framework; the app is small enough that a
  plain `Main` is clearer.
