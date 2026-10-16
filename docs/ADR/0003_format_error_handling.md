---
title: Format error handling
description: What the console application prints and returns when an input file cannot be parsed.
---
# Format error handling

## Context

The parser rejects input that does not follow the format rules (non-numeric count,
fewer lines than declared, odd number of fields, empty field). A malformed file is a
different situation from a valid file that describes an invalid installation, and the
console output has to make the difference visible without breaking a test runner that
compares results per file.

## Options

1. **Print `FAIL` for the file on stdout, details on stderr, exit code 2.** The runner
   always gets a result line; the exit code distinguishes "broken file" from "invalid
   installation". Risk: a runner that requires exit code 0 treats the whole run as failed.
2. **No result line for the file, details on stderr, continue with the remaining files,
   non-zero exit code at the end.** stdout contains only genuine results. The runner sees a
   missing line for the broken file.
3. **Print `FAIL`, exit code 0, no diagnostics.** Simplest, but a parse bug becomes
   indistinguishable from a real `FAIL`.

## Decision

Option 2. A file that cannot be parsed is reported on stderr with the file name, line
number and reason. The remaining files are still processed. The process exits with a
non-zero code when at least one file failed to parse or on a usage error.

## Consequences

- `README.md` already documents this behaviour.
- `tech_design.md` section 3.3 and the exit code table in section 4 must be rewritten
  once this record is accepted.
- The parser must carry the line number in its exception so the message is useful.
