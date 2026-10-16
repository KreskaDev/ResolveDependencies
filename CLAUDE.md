# Project

- **Do not guess intent.** When a request is ambiguous or the task statement does not cover a
  case, ask instead of assuming. If you must proceed, state the assumption explicitly.
- **Granular commits.** One logical change per commit (for example: docs, scaffolding, parser,
  validator, tests). Do not mix unrelated changes. Commit only when asked.
- **English only.** All files, code, comments, documentation and commit messages are written
  in English, regardless of the language used in conversation.

## Documentation

- **File names are snake_case.** Every file under `docs/` is named in lower snake_case,
  for example `tech_design.md`. `docs/story/` keeps the original file names of the task
  material.
- **Documents describe the current state.** Every file in `docs/` describes how things
  are now, written as if from scratch today. No decision logs, no dated entries, no
  "previously" or "changed from" wording, no draft/status headers. History lives in git.
- **Open decisions are ADRs.** Anything that is undecided or contested gets its own
  Architecture Decision Record in `docs/ADR/NNNN_short_title.md` (snake_case, one
  decision per file) with the sections `Context`, `Options`, `Decision` and
  `Consequences`. An ADR describes a possible future state, never the current one.
  Documents never link to or mention ADRs.
- **Numbers are never reused.** `NNNN` is the largest number ever used in the repository
  (check git history, not only the current folder) plus one.
- **Resolving an ADR deletes it.** The repository owner decides. Claude applies the
  decision only on an explicit instruction naming the ADR. The resolution is one commit
  that removes the ADR file and, in the same commit, updates the affected documents so
  they state the decision as current fact. A rejected ADR is removed without changing
  the documents. The commit message carries the rationale: context, options considered
  and the decision. Documents do not repeat it.

## File header (front matter)

Every Markdown file under `docs/` starts with a YAML front matter block, followed by a
single `#` heading equal to `title`. Nothing else goes before the heading.

```yaml
---
title: Architecture
description: Structure of the solution, components, algorithm and testing strategy.
related:
  - docs/tech_design.md
  - docs/story/ResolveDependencies.md
---
```

- `title`: short name of the document, two to four words, no file name, no version.
- `description`: one sentence saying what the reader will find inside. Used to decide
  which document to open, so it must be specific.
- `related`: optional list of repository-relative paths this document depends on or
  complements. Omit the key when there is nothing to link.

ADRs use the same block with `title` and `description` only. They carry no status, no
date and no `related` list; the git history records when they were created.

Files outside `docs/` (`README.md`, `CLAUDE.md`) have no front matter. `docs/story/` holds the
original task material verbatim and is exempt as well.
