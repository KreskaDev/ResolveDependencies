# Project

- **Do not guess intent.** When a request is ambiguous or the task statement does not cover a
  case, ask instead of assuming. If you must proceed, state the assumption explicitly.
- **Granular commits.** One logical change per commit (for example: docs, scaffolding, parser,
  validator, tests). Do not mix unrelated changes. Commit only when asked.
- **English only.** All files, code, comments, documentation and commit messages are written
  in English, regardless of the language used in conversation.

## Documentation

- **Documents describe the current state.** Every file in `docs/` describes how things
  are now, written as if from scratch today. No decision logs, no dated entries, no
  "previously" or "changed from" wording, no draft/status headers. History lives in git.
- **Open and notable decisions are ADRs.** Anything that is undecided, contested or worth
  justifying gets its own Architecture Decision Record in `docs/ADR/NNNN-short-title.md`
  (sequential number, kebab-case title, one decision per file) with the sections
  `Context`, `Options`, `Decision`, `Consequences`.
- **Status is owned by the human.** A new ADR starts as `Proposed`. Only the repository
  owner moves it to `Accepted`, `Rejected` or `Superseded by NNNN`. Claude may create and
  edit `Proposed` records but never changes a status.
- **Documents follow accepted ADRs.** Once an ADR is `Accepted`, update the affected
  documents in the same change so they state the decision as current fact and link the
  ADR for the rationale. Documents do not repeat the reasoning, and never mention
  `Proposed` or `Rejected` decisions.

## File header (front matter)

Every Markdown file under `docs/` starts with a YAML front matter block, followed by a
single `#` heading equal to `title`. Nothing else goes before the heading.

```yaml
---
title: Architecture
description: Structure of the solution, components, algorithm and testing strategy.
related:
  - docs/tech-design.md
  - docs/story/ResolveDependencies.md
---
```

- `title`: short name of the document, two to four words, no file name, no version.
- `description`: one sentence saying what the reader will find inside. Used to decide
  which document to open, so it must be specific.
- `related`: optional list of repository-relative paths this document depends on or
  complements. Omit the key when there is nothing to link.

ADRs use the same block with two additional keys:

```yaml
---
title: Parser lives in Core
description: Where the text-to-model parsing belongs and why.
status: Proposed
date: 2026-10-16
---
```

- `status`: one of `Proposed`, `Accepted`, `Rejected`, `Superseded by NNNN`. This is the
  only place in the repository where a status is recorded.
- `date`: the day the record was created or its status last changed, ISO 8601.

Files outside `docs/` (`README.md`, `CLAUDE.md`) have no front matter. `docs/story/` holds the
original task material verbatim and is exempt as well.
