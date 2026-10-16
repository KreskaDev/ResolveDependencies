---
title: Tech Design
description: Text format of the input files, command line, output files, console output and exit codes, plus the domain types and components of the Core library and the console application.
related:
  - docs/architecture.md
  - docs/algorithm_spec.md
---
# Tech Design

## Input file format

The format from `docs/story/ResolveDependencies.md`, extended with the deviations found in
`docs/story/goldenData/`. One file holds one installation request.

```
N                        number of packages to install
p,v                      x N
M                        number of dependency lines
p1,v1,p2,v2[,p3,v3...]   x M
```

| #  | Rule | Source |
|----|------|--------|
| F1 | Fields are separated by a comma. Package and version contain no comma. | task statement |
| F2 | A dependency line has an **even number of fields, at least 4**. The first pair is the dependent package, every following pair is one of its requirements. `B,2,A,1,C,1` means `B,2 -> A,1` and `B,2 -> C,1`. | 002 (deviation from the task statement) |
| F3 | The dependency section (the `M` line and what follows) **may be absent**. Treated as M = 0. | 008 |
| F4 | Line endings `\n`, `\r\n` and a missing trailing newline are all equivalent. | goldenData |
| F5 | Empty lines (after trim) at the end of the file are ignored. Empty lines inside a section are a format error. | 003, 007 |
| F6 | Leading and trailing whitespace of a line is trimmed. Whitespace inside a field is **not** trimmed; it is part of the name. | assumption |
| F7 | N and M must be non-negative integers and must match the actual number of lines in the section. A mismatch is a format error. | goldenData (all consistent) |
| F8 | An empty field (e.g. `,1` or `A,`) is a format error. | assumption |

An install line has exactly two fields; any other count is a format error.

## Format error handling

A format error is any input the parser rejects: an empty line inside a section (F5), a
count that is not a non-negative integer or does not match the section (F7), an empty
field (F8), an odd number of fields in a dependency line (F2), or an install line with a
field count other than two. The parser stops at the first error and reports its line
number and reason.

A format error is a regular per-file result, not a program failure. The file gets an
output file with `ERROR` and the reason, the remaining files are still processed and the
exit code stays `0`. Only invalid arguments and I/O errors make the program exit with a
non-zero code.

## Command line

```
app <input-folder> [output-folder]
```

Every `*.txt` file in the input folder whose name starts with `input` is processed in name
order. The output folder defaults to the input folder. Any other argument shape is a usage
error.

## Output files

For every `inputNNN.txt` the program writes `outputNNN.txt`: the input file name with the
`input` prefix replaced by `output`, placed in the output folder. An existing output file
is overwritten.

| Result | File content |
|--------|--------------|
| Installation valid | `PASS` |
| Installation invalid (a package is required in more than one version) | `FAIL` |
| Input file malformed | `ERROR`, followed by a second line `line <n>: <reason>` |

Result keywords are upper case. Every line is terminated with `\n`.

## Console output

Standard output gets one summary line per file, `inputNNN.txt: PASS`, `FAIL` or `ERROR`,
followed by diagnostic lines indented with two spaces:

- for `FAIL`, one line per conflicting package, `<package>: <version>, <version>, ...`,
  versions in the order they were met;
- for `ERROR`, the reason, `line <n>: <reason>`;
- for `PASS`, nothing.

Diagnostics for usage and I/O errors go to standard error.

## Exit codes

| Code | Meaning |
|------|---------|
| 0 | all input files processed; every file has an output file, including `ERROR` results |
| 1 | invalid arguments or I/O error (folder missing, file unreadable, output folder not writable) |

## Domain model

| Type                  | Kind            | Purpose |
|-----------------------|-----------------|---------|
| `PackageVersion`      | `record struct` | `(Name, Version)` pair. Value equality is what the whole algorithm relies on. |
| `Dependency`          | `record`        | `Dependent` requires `Requirement`, both `PackageVersion`. |
| `InstallationRequest` | `record`        | Parsed input: list of packages to install plus list of dependencies. |
| `Conflict`            | `record`        | A package name with the set of versions required of it. |
| `ValidationResult`    | `record`        | List of `Conflict`s; valid when the list is empty. Mapped to `PASS` / `FAIL` only at the output boundary. |

## Components

```
 text ──► InputParser ──► InstallationRequest ──► DependencyValidator ──► ValidationResult
```

### Core

Pure functions, no I/O, no knowledge of files or folders.

- **`InputParser`** (own namespace) implements rules F1-F8. It is the only place that
  knows the text format. It receives the raw text of one input file and returns an
  `InstallationRequest`, or throws a `FormatException` carrying the line number and reason.
- **`DependencyValidator`** takes an `InstallationRequest` and returns a `ValidationResult`
  with every conflict found, following `docs/algorithm_spec.md`. Pure function, no
  exceptions for valid-but-failing inputs. Assumes the data is structurally valid and
  does not validate the format; may check invariants (non-empty strings) and throw a
  programming error if they are broken.
- **`DependencyResolver`** (facade) composes parser and validator:
  `Resolve(string input) => ValidationResult`. The one entry point the console application
  calls. The validator stays available directly for callers that already hold an
  `InstallationRequest`.

### Console application

Responsible for arguments, enumerating and reading the input files, calling the facade,
writing the output files and the console summary, and exit codes. Contains no parsing code
and no domain logic.
