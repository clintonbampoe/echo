# Ports

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

| Port | Service | Reachable from outside? | Dev | Prod |
|---|---|---|---|---|
| 8080 | nginx | YES | YES | — |
| 80 | nginx | YES | — | YES |
| 5173 | client (Vite) | YES (dev convenience) | YES | — |
| 5025 | api | YES (dev convenience) | YES | — |
| 5432 | db (Postgres) | YES in dev, NO in prod | YES | — |

## Flow

**Production:** All traffic → nginx (port 80) → `/api` routes to `api`, everything else serves web app.

**Development:** nginx (8080) proxies `/api` to `api:5025`, passes web app to `client:5173`. Direct access to Vite (5173) and API (5025) also works for debugging.