# Docs overhaul plan

Date: 2026-11-07. Branch: `docs/overhaul`.

The docs describe a setup that no longer exists: two servers, 2 vCPU, port 80
only, no HTTPS, no release process. Meanwhile the biggest things we built
(releases, server setup, Gatus, rollback) have no page at all. This plan fixes
both, in an order that puts the reader's most urgent questions first.

## What the audit found

Stale, wrong today:

| Where | Problem |
|---|---|
| `infrastructure/compose.md` | 2 vCPU / 4 GB box, "monitoring on another machine", resource table from the old sizing, prod ports "80 only" |
| `infrastructure/observability.md` | "Production: two servers" section, `create-monitor-role.sh` (the script is now an init script) |
| `infrastructure/observability-plan.md` | a plan whose decisions were reversed ("Two servers") |
| `infrastructure/persistence.md` | Postgres tuning for 2 vCPU, `max_parallel_workers_per_gather` 0 |
| `infrastructure/ports.md`, `topology.md` | no 443, no `certbot`, no `status`, no Grafana route |
| `infrastructure/build.md` | `linux-arm64` and `TARGETARCH`; the build is amd64 only |
| `infrastructure/backups.md` | no heartbeat to Gatus, no `pipefail` |
| `api/api-usage.md`, `backend/README.md`, `backend/architecture.md` | still talk about controllers; the backend uses minimal APIs |
| `AGENTS.md` | branch examples and workflow still say Linear-style names, no release process |
| `docs/README.md` | conventions contradict how we work now (byline on every page, "never write a doc until stable") |
| `docs/index.md`, `infrastructure/index.md` | two lists of the same pages; `observability*.md` missing from the first |

Missing entirely:

- how a release works, how to roll back, how to set up a server
- HTTPS and certificates, the status page, the deploy key and its limits
- limits of the setup (one server, about 800 req/s clean on 4 vCPU)
- the incidents we hit this week (an empty troubleshooting page sits there)

## Standards we adopt

From the research (Diataxis, Google's developer style guide, Write the Docs,
and how Litestream, Caddy, FastAPI, Bubble Tea and SQLite write) combined with
the project's own rules.

Structure
1. Four kinds of page, never mixed: tutorial (learn by doing one path),
   how-to (get a job done), reference (look something up), explanation (why).
2. Group the index by what the reader wants, not by folder history:
   start here, how-to, reference, explanation.
3. Every page has one job. If a how-to needs a paragraph of why, link to the
   explanation page.
4. Reference pages mirror the thing they describe and use one format for every
   entry.
5. Label old material instead of leaving it to mislead: "obsolete" with a
   reason, or delete it.

Writing
6. First paragraph says what the thing is, what it guarantees and what it is
   not.
7. Commands are bare and copy-pasteable, with the expected output right after.
   Say where to run them ("on the server, as root").
8. Conditions before instructions. Second person, active voice, sentence-case
   headings, descriptive link text.
9. Limits are numbers with a date and a reason, in their own section. Every
   setup page that has them gets a short "when not to use this" list.
10. Operator-visible side effects get their own section (things that only
    happen on first start, things not to delete while running).
11. Troubleshooting headings are the exact error text, quoted, so a search
    lands on them. Every entry comes from a real incident.
12. No bylines or "last updated" lines. Git has the history; hand-kept dates
    go stale first.

Project rules
13. Lean: one file per topic, delete before adding, no config for values that
    never change.
14. Docs are code: they change in the same PR as the thing they describe, and
    CI checks that relative links resolve.
15. No AI tells (humanizer skill): no staged openers, forced triads, sales
    words.
16. Wrap near 80 columns; keep the repo's markdown style.

## Target layout

Keep the current folders; rename only where a page changes kind.

```
docs/
  README.md                  one index, grouped by reader intent
  getting-started.md         tutorial: run Echo locally, one path
  how-to/
    release.md               cut a release, watch it, roll back
    set-up-a-server.md       fresh server to first release (from today)
    restore-a-backup.md
    rotate-jwt-keys.md       (moved from infrastructure/jwt-rotation.md)
    run-a-load-test.md       (points at infra/loadtest/README.md)
  reference/
    environment.md           every variable, by service
    services.md              services, ports, volumes, compose files
    nginx.md                 files, routes, headers
    ci-and-workflows.md      what each workflow does and when
  explanation/
    topology.md              why one server, build once, memory caps
    observability.md         what is collected, where it goes, dashboards
    status-page.md           what Gatus checks, heartbeats, how jobs report
    limits.md                what this setup can carry, and what it can't
  troubleshooting.md         verbatim error strings from real incidents
  backend/  frontend/  api/  performance/   (kept; trimmed to match code)
```

## Disposition of the current pages

| Page | Action |
|---|---|
| `README.md`, `index.md`, `infrastructure/index.md` | merge into one `README.md` index; delete the other two |
| `getting-started.md` | tighten into a one-path tutorial with expected output |
| `infrastructure/compose.md` + `ports.md` + `persistence.md` | merge into `reference/services.md`, drop the 2 vCPU sizing |
| `infrastructure/topology.md`, `build.md` | rewrite as `explanation/topology.md`; drop arm64 |
| `infrastructure/web-app.md` | rewrite as `reference/nginx.md` (add 443, certs, status host) |
| `infrastructure/environment.md` | rewrite as `reference/environment.md` from `.env.example` |
| `infrastructure/observability.md` | rewrite for one server; keep dashboards list |
| `infrastructure/observability-plan.md` | delete (decisions are made; the page misleads) |
| `infrastructure/backups.md`, `migrations.md`, `logging.md` | fold facts into reference or how-to pages |
| `infrastructure/jwt-rotation.md` | move to `how-to/rotate-jwt-keys.md` |
| `infrastructure/troubleshooting.md` | rewrite with this week's real incidents |
| `api/*`, `backend/*` | fix controller wording, check against code |
| `frontend/*` | check against code; mark components inventory if it lags |
| `performance/*` | leave; add a banner that numbers are for the old 2 vCPU box |
| `superpowers/*` | leave (plans and specs, dated) |
| new: how-to/release, set-up-a-server, restore-a-backup, run-a-load-test | write from what exists |
| new: explanation/status-page, limits; reference/ci-and-workflows | write |
| `AGENTS.md`, `CONTRIBUTING.md`, `docs/README.md` conventions | update to the rules above |

## Order of work

Stop and commit after each step; one logical commit each.

1. Conventions and the single index (`docs/README.md`, `AGENTS.md`).
2. Release and server how-tos (the new knowledge, highest value).
3. Reference: services, environment, nginx, workflows.
4. Explanation: topology, observability, status page, limits.
5. Troubleshooting from real incidents.
6. Delete or move what the table says; fix links.
7. Backend and frontend checks against the code.
8. Link check in CI (the `infra` job already runs; add one script).

## Out of scope

- Code changes, apart from the link check script.
- `performance/` numbers: the load test hasn't been rerun on the new box, so
  rewriting them would invent facts.
- Frontend component inventory beyond a staleness check.

## Done when

- Nothing in `docs/` mentions two servers, 2 vCPU or controllers.
- A new person can go from a fresh Hetzner server to a running release using
  only `how-to/set-up-a-server.md` and `how-to/release.md`.
- Every relative link resolves, and CI fails when one stops.
