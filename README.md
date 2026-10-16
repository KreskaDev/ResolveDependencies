# Resolve dependencies

## Goal

The program receives an installation description (a list of packages to install plus
dependency rules) and answers whether the installation is valid. An installation is
invalid when, after taking the transitive closure of the dependencies, any package is
required in more than one version.

Result: `PASS` or `FAIL`.

- The problem statement is in [`ResolveDependencies.md`](docs/story/ResolveDependencies.md).
- The design is described in [`architecture.md`](docs/architecture.md), [`tech_design.md`](docs/tech_design.md) and [`algorithm_spec.md`](docs/algorithm_spec.md).

## Build

Requires .NET SDK 10.0 or newer.

```
dotnet build src
```

## Run

The console application takes two arguments: the input folder and, optionally, the output
folder. Every file named `input*.txt` in the input folder is processed in name order. For
each `inputNNN.txt` the application writes `outputNNN.txt` with the result and prints a
summary line to standard output, followed by the conflicting packages for `FAIL` or the
reason for `ERROR`.

```
dotnet run --project src/ResolveDependencies.Console -- <input-folder> [output-folder]
```

Without the second argument the output files are written next to the input files. Existing
output files are overwritten.

Example, using the test data shipped with the repository without touching the expected
results:

```
dotnet run --project src/ResolveDependencies.Console -- docs/story/goldenData out
```

Standard output:

```
input000.txt: PASS
input001.txt: FAIL
  B: 2, 1
input002.txt: ERROR
  line 4: expected an even number of fields, got 3
...
```

An output file contains `PASS` or `FAIL`. A file that cannot be parsed gets `ERROR` on the
first line and the reason with the line number on the second line, for example:

```
ERROR
line 4: expected an even number of fields, got 3
```

The exit code is `0` when every input file was processed, including files that produced
`ERROR`. It is non-zero only for invalid arguments or I/O errors.

## Test

```
dotnet test src
```

The test suite contains unit tests for the parser and validator and golden tests that run
every `docs/story/goldenData/inputNNN.txt` and compare the result with `docs/story/goldenData/outputNNN.txt`. To add a
case, add a new `inputNNN.txt` / `outputNNN.txt` pair to `docs/story/goldenData/`.

## Before committing

- `dotnet build src` and `dotnet test src` must pass with zero warnings.
- The files in `docs/story/goldenData/` are the expected results. When running the console
  application against that folder, give an output folder so they are not overwritten.
