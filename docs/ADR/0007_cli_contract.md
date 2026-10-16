---
title: Command-line contract
description: What the console application accepts as input and how it prints results.
---
# Command-line contract

## Context

The task does not say how the program is invoked. The golden data is a folder of
`inputNNN.txt` / `outputNNN.txt` pairs. `README.md` and `architecture.md` describe a
folder-based interface; `tech_design.md` describes a single-file or stdin interface with a
bare `PASS` / `FAIL` on stdout.

## Options

1. **Folder argument.** `app <folder>` processes every `input*.txt` in name order and
   prints `<file name>: PASS|FAIL` per file. One run covers all golden cases; the output
   is self-describing. Does not match a runner that pipes one file at a time.
2. **Single file or stdin.** `app <file>` or `app < file` prints exactly `PASS` or
   `FAIL`. Matches a classic per-case runner; running all cases needs a shell loop.
3. **Both.** A folder argument behaves as option 1, a file argument or stdin as option 2.
   Covers both runner styles at the cost of a slightly larger argument parser.

## Decision

Option 1, as already documented in `README.md`.

## Consequences

- `tech_design.md` sections 4 and 5.2 describe option 2 and must be aligned once this
  record is accepted.
- Running the application against `docs/story/goldenData/` never writes into that folder.
- If the invocation style of the external runner becomes known and differs, this record
  is superseded rather than edited.
