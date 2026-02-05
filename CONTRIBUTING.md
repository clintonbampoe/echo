# Contributing

This file explains how to contribute to the Echo repository.  
It covers expectations, branch rules, commit conventions, pull request requirements, documentation responsibilities, and issue categories.  
The repo contains documentation, source code, legacy assets (Excel), media, and other artifacts; these guidelines apply to the whole project.

---

## Purpose

Provide a simple, consistent process so collaborators can add, change, or remove content safely and traceably.  
We are starting with documentation first; the same rules apply when code and other assets are added.

---

## Scope

These guidelines apply to:

- Documentation (product and process docs)
- Source code and tests
- Legacy artifacts (Excel sheets, CSVs)
- Media and design assets
- Repo configuration and automation files

---

## Core Expectations

- Work in branches; do not push directly to main.
- Keep changes focused and descriptive.
- Update documentation or process notes whenever you change behavior, data models, or interfaces.
- Treat documentation and process artifacts as first-class deliverables.
- Be respectful and clear in issues and PR descriptions.

### Documentation Rules

We maintain two complementary documentation types. Always update the appropriate one.

- **Product Documentation**
  
  - What we are building and why.
  - User-facing requirements, feature descriptions, acceptance criteria.

- **Process Documentation**
  
  - Technical details, implementation notes, data models, developer workflows, and runbooks.
  - This is the authoritative source for how things are implemented and maintained.

### Guidelines

- Update product docs when requirements or user-facing behavior change.
- Update process docs when implementation, schema, or operational details change.
- Keep `structure.md` as the navigation map and `overview.md` as the high-level guide. Do not duplicate file lists across docs.

---

## Version Control

### Commit Message Prefixes

Use a short prefix followed by a colon and a concise message. This makes history scannable.

- **Required prefixes**

  - **added:** New files or features added.
  - **modified:** Changes to existing content that are not new features.
  - **fix:** Bug fixes or corrections.
  - **feat:** New feature or capability (larger than added).

- **Recommended additional prefixes**

  - **docs:** Documentation-only changes.
  - **chore:** Repo maintenance, tooling, or housekeeping.
  - **refactor:** Code or structure changes that do not alter behavior.
  - **test:** Tests added or updated.
  - **perf:** Performance improvements.
  - **revert:** Revert a previous commit or PR.

- **Example**

```markdown
added: requirements/functional-requirements/attendance.md
modified: overview.md to clarify section purpose
fix: correct typo in structure.md
```

---

### Branching and Workflow

- **main is protected**. Direct pushes to `main` are not allowed.
- Create a branch for your work using one of these prefixes:
  - `feature/` for new features or large additions
  - `docs/` for documentation-only changes
  - `fix/` for bug fixes or corrections
  - `chore/` for maintenance tasks (dependencies, housekeeping)
- Keep branches up to date with `main` and resolve conflicts before merging.

---

### Pull Requests

- Open a PR from your branch into `main`.
- During the documentation-first phase PR approvals are not required for docs-only PRs to speed progress.
- Once implementation begins, PRs that touch code or production artifacts will require at least one approval from a maintainer before merge.
- PR description must include:
  - Short summary of what changed and why.
  - **Files changed** with commit-style prefixes (see Commit Message Prefixes). Example: `changes, added: requirements/functional-requirements/assets.md`
  - Link to related issue or requirement if applicable.
- Assign at least one reviewer when possible.

---

### Issues and Labels

Use clear labels and brief descriptions so triage is fast.

- **bug** Fix a defect or incorrect behavior.
- **enhancement** Improve an existing feature or add a small improvement.
- **feature** New capability or major addition.
- **docs** Documentation changes or requests.
- **process** Changes to developer workflows, runbooks, or architecture notes.
- **design** UX or visual design work.
- **question** Clarification or decision needed.

When opening an issue include context, expected outcome, and suggested next steps.

---

### Reviews and Approvals

- Docs-only PRs can be merged without approvals during the planning phase to accelerate documentation.
- Code, infra, or production-impacting PRs require at least one maintainer approval once implementation begins.
- Reviewers focus on clarity, correctness, and traceability to requirements or issues.

---

## Security and Sensitive Data

- Never commit secrets, credentials, or PII.
- If you find sensitive data in the repo, notify maintainers immediately and open a private issue.

---

## Communication

- Use issues and PRs for asynchronous discussion and decisions.
- Major design or scope changes should be proposed in an issue or design doc before implementation.
- Keep PR descriptions and issue bodies factual and concise.

---

## Final Notes

- These guidelines are intentionally pragmatic and lightweight to support rapid documentation work now and stricter controls later.
- If you are unsure where to document something, prefer **process documentation** for technical details and **product documentation** for user-facing decisions.

  Thank you for contributing.
