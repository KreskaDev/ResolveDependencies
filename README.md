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

The console application takes the input folder and, optionally, the output folder, either
positionally or as `--input` and `--output` options. Every file named `input*.txt` (lower
case, exactly as in the golden data) in the input folder is processed in name order. For
each `inputNNN.txt` the application writes `outputNNN.txt` with the result and prints a
summary line to standard output, followed by the conflicting packages for `FAIL` or the
reason for `ERROR`.

```
dotnet run --project src/ResolveDependencies.Console -- <input-folder> [output-folder]
dotnet run --project src/ResolveDependencies.Console -- --input <folder> [--output <folder>]
```

Without an output folder the output files are written next to the input files. A missing
output folder is created. Existing output files are overwritten.

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
input002.txt: FAIL
  A: 1, 2
...
```

An output file contains `PASS` or `FAIL`. A file that cannot be parsed gets `ERROR` on the
first line and the reason with the line number on the second line. The golden data has no
such file; a dependency line with an odd number of fields, for example, gives:

```
ERROR
line 4: expected an even number of fields, got 3
```

and the summary line `inputNNN.txt: ERROR` followed by the same reason on the console.

The exit code is `0` when every input file was processed, including files that produced
`ERROR`. It is non-zero only for invalid arguments or I/O errors.

## Test

```
dotnet test src
```

The test suite contains unit tests for every component of Core and Console (command line
parser, sanitizer, parser, file processor, report, graph builder and resolver) and
golden tests, one per `docs/story/goldenData/inputNNN.txt`: the sanitizer gets the file
text transcribed into the test, the resolver gets the request built in code, and the
verdict comes from `docs/story/goldenData/outputNNN.txt`. To add a case, add a new
`inputNNN.txt` / `outputNNN.txt` pair to `docs/story/goldenData/` and the matching tests.

## Before committing

- `dotnet build src` and `dotnet test src` must pass with zero warnings.
- The files in `docs/story/goldenData/` are the expected results. When running the console
  application against that folder, give an output folder so they are not overwritten.
