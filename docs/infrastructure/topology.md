# Containers & Topology

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

![Compose stack topology](./diagrams/ComposeStack.excalidraw.svg)

## Services

| Service    | Role                   | Notes                                                                                                                                     |
| ---------- | ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `api`      | Backend (ASP.NET Core) | Dev: fast rebuild (skips restore). Prod: compiled. Nginx routes traffic after healthcheck passes.                                         |
| `client`   | Frontend (Vite)        | **Dev only**. Runs on port 5173. Nginx passes through. Not present in prod.                                                               |
| `db`       | PostgreSQL 18          | Data in `postgres-data` volume. Prod: only `api` can reach it.                                                                            |
| `nginx`    | Single front door      | Routes `/api`, `/scalar`, `/swagger` → `api`. Everything else → web app. Dev: passes to `client`. Prod: serves built files + CSP headers. |
| `migrator` | Runs EF migrations     | For controlled updates in CI/CD or when API is offline.                                                                                   |
| `backup`   | Scheduled DB dumps     | Uses Supercronic for reliable env var passing.                                                                                            |

## Health Endpoints

Both are public and unauthenticated.

- **`/api/health/live`** — "is this process up?" No external deps. Database outage doesn't trip it. Used by compose healthcheck.
- **`/api/health/ready`** — "should traffic be routed here?" Checks: (1) DB reachable, (2) all migrations applied. 503 with pending migrations = run `migrator`.