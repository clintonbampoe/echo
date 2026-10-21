# AGENTS.md

Guidance for OpenCode and any other agent working in this repository.

**Read [`CLAUDE.md`](CLAUDE.md) first** — it holds the project summary, architecture, module
layout, and conventions. It applies to every agent, not just Claude Code.

## Performance work

**[`docs/Performance.md`](docs/Performance.md)** is the live plan. Read it before touching
query code, indexes, Postgres configuration, or the nginx / Docker / rate-limiting stack.
It records the measured baselines, the order of work, the reasoning behind it, and the
landmines that are deliberately parked.

Measurements go in **[`docs/Performance-log.md`](docs/Performance-log.md)** — one entry per
move, five lines each (hypothesis, before, after, learned, surprise). Keep it in the same
commit as the change it describes.

## Other references

| File | Covers |
|---|---|
| `docs/Performance.md` | The delivery plan, the four walls, the 7 moves |
| `docs/Performance-log.md` | The measurement log |
| `docs/Infrastructure.md` | How the Docker/nginx/Postgres stack fits together |
| `docs/frontend/Architecture.md` | Frontend layering rules and folder layout |
| `docs/api/ApiUsage.md` | API usage notes |
| `docs/diagrams/` | Excalidraw source for the architecture diagrams |

## Working agreements

- `.env` lives at the repo root and is required to run the API. `.env.example` is the
  template. Never commit `.env`.
- Work on a branch. Convention: `prefix/scope-identifier-number-description`, e.g.
  `feat/frontend-clnt-11-implement-client-app-login-page`. Linear identifiers use a
  four-letter prefix like `CLNT-N`.
- Update the technical docs, schemas and diagrams **immediately** when making structural
  changes. A change that alters a query, an index, a schema shape or the stack topology
  needs the matching update in `docs/`.
- Add tests alongside every fix. `Echo.IntegrationTests` is currently a stub and is not in
  `Echo.slnx` — do not rely on it as a safety net.
- `dotnet ef` is currently broken design-time: `DesignTimeDbContextFactory` reads the
  connection string `"Default"` while the runtime uses `"echodb"`.
