---
title: Multi-pair dependency line
description: How a dependency line with more than four fields is interpreted.
status: Proposed
date: 2026-10-16
related:
  - docs/story/ResolveDependencies.md
  - docs/story/goldenData/input002.txt
  - docs/tech-design.md
---
# Multi-pair dependency line

## Context

`docs/story/ResolveDependencies.md` defines a dependency line as `p1,v1,p2,v2`. The golden file
`docs/story/goldenData/input002.txt` contains the line `B,2,A,1,C,1` followed by `C,1,A,2`, with
expected output `FAIL`. `B,2` is the only installed package.

## Options

1. **Use the first four fields, ignore the rest.** `B,2 -> A,1` only. `C,1` is never
   reached, so the result is `PASS`. Contradicts the expected output.
2. **Treat the line as a format error.** The file is rejected; whether that counts as
   `FAIL` depends on the format error handling decision. Matches the expected output only
   by accident.
3. **Parse as a list of pairs.** The first pair is the dependent, each following pair is
   one requirement: `B,2 -> A,1` and `B,2 -> C,1`. `C,1 -> A,2` then conflicts with
   `A,1`, giving `FAIL` for the right reason. A line must have an even number of fields,
   at least four.

## Decision

Option 3. The format rule is documented in `tech-design.md` (F2) and in the input
format table of `architecture.md`. This interpretation was not confirmed with the task
author; it is the only one that produces the expected result for a semantic reason.

## Consequences

- The parser accepts any even field count greater than or equal to four and rejects
  odd counts.
- If the task author later confirms option 1 or 2, `input002` must be re-examined and
  the parser rule changed.
