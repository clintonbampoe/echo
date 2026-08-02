# Echo Docs

**Written by:** @clintonbampoe
**Last updated:** 2026-07-31 by @clintonbampoe

---

## Conventions

- **One file per topic**, not one file per code layer. A doc should tell the whole story for something (e.g. "how auth works"), not just describe the controllers or just the entities.

- **Don't write a doc until the thing it describes is stable.** A doc for a feature that's still actively changing goes stale before it's even useful.

- **Diagrams over prose wherever a picture explains it faster.** Excalidraw, exported as SVG with the scene embedded — renders in markdown, and can be dragged back into Excalidraw to edit later. All diagrams live in `docs/diagrams/`, named after the concept they show (`compose-stack.svg`, not a date or a version number).

- **Filenames are camelCase** (`Setup.md`, `Infrastructure.md`) — matches the index and avoids case-sensitivity surprises on Linux/Windows/GitHub.

- **Diagnose-and-document:** when you fix something that took real time to figure out, add an entry to the relevant doc's troubleshooting section, in the format that section specifies. Direct commits are fine; the goal is never solving the same problem twice, not gating every entry through review.

- **Every doc starts with a byline**

```markdown
**Written by:** @handle
**Last updated:** date by @handle
```

  right under the title, before any content. Whoever edits a doc updates the "Last updated" part in the same change.

- **Update the table below whenever a doc is added, rewritten, or goes stale.** This index is only useful if it's honest.

---

## Everything else

Every doc that previously existed under `docs/api/` and `docs/database/`, and the setup section of `backend/README.md`, has been removed. All of it was stale and describing a setup/architecture that no longer reflects the real codebase. Rewrites happen incrementally, one topic at a time — check the table above for what's actually been redone.

**You can look at [Table of Contents](./Index.md) to navigate the documentation.