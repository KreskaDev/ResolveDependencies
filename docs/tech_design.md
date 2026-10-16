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

Rules F4, F5 and F6 are applied by the sanitizer, which turns the raw text into trimmed
lines without trailing empty lines. The remaining rules are applied by the parser, which
works on those lines; an empty line inside a section reaches the parser and is rejected
there.

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
app --input <folder> [--output <folder>]
```

Each folder may be given positionally (input first, output second) or by its option, in
any order, and the two styles may be mixed in one call. Each folder is given at most
once; the input folder is required, the output folder defaults to the input folder. An
option without a value, an unknown option (any argument starting with `--`), a third
positional argument or a folder given twice is a usage error.

Every file in the input folder whose name starts with `input` and ends with `.txt` is
processed in ordinal name order. The match is exact, as in `docs/story/goldenData/`:
`Input000.txt` or `input000.TXT` is not an input file on any platform.

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
 text ──► InputSanitizer ──► lines ──► InputParser ──► InstallationRequest ──► DependencyResolver ──► ValidationResult
 └──────────────── Console ────────────────┘                                  └───── Core ─────┘
```

### Core

No I/O, no knowledge of files, folders or the text format.

All services are instance classes so callers and tests work with objects. None of them
throws for a valid-but-failing input; they assume the request is structurally valid and do
not validate the format.

- **`DependencyGraph`** is the directed graph of the rules: `RequirementsOf(PackageVersion)`
  returns the package versions a version requires, in rule order, or an empty list for a
  version without rules.
- **`DependencyGraphBuilder`** is the domain service building the graph from the
  `Dependency` list. Several rules for one dependent are read together as its requirement
  list.
- **`InstallationValidator`** is the domain service checking the semantic rules:
  `Validate(packages, graph) => ValidationResult` runs the traversal of
  `docs/algorithm_spec.md` from the install list and returns every conflict found.
- **`DependencyResolver`** composes the two: `Resolve(InstallationRequest) =>
  ValidationResult` builds the graph from the request's rules and validates the request's
  packages against it. It is the entry point the console application calls.

### Console application

Responsible for arguments, enumerating and reading the input files, turning their text
into the Core contract, calling the domain service, writing the output files and the
console summary, and exit codes. Contains no domain logic.

- **`InputSanitizer`** implements rules F4-F6. `Sanitize(string text) =>
  IReadOnlyList<string>` returns the trimmed lines of one input file without trailing
  empty lines. Empty lines inside the file are kept so the parser can report them.
- **`InputParser`** implements the remaining format rules. It is the only place that
  knows the line format. It receives the sanitized lines and returns an
  `InstallationRequest`, or throws an `InputFormatException` (a `FormatException`)
  carrying the one-based line number and the reason; its message is the
  `line <n>: <reason>` text written to the output file.
- **`InputFileProcessor`** runs the text of one file through sanitizer, parser and
  `DependencyResolver` and returns a `FileReport`. It catches `InputFormatException` and
  turns it into an `ERROR` report; nothing else is caught.
- **`FileReport`** is the outcome of one file: a `Verdict` (`Pass`, `Fail`, `Error`), the
  conflicts for `Fail` and the reason for `Error`. It renders itself as the output file
  content and as the console lines for a given file name.
- **`CommandLineParser`** turns the arguments into `CommandLineOptions` (input and output
  folder) following the command line rules above, or throws a `CommandLineException`
  with the reason. It knows nothing about the file system.
- **`Program`** owns the process: calls the command line parser and prints the reason and
  the usage on a `CommandLineException`, checks that the input folder exists, creates the
  output folder, enumerates `input*.txt` in ordinal name order, reads and writes the
  files, and sets the exit code. I/O failures are written to standard error.
