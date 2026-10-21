# Web App & nginx

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## nginx Image Targets

`infra/nginx/Dockerfile` has two targets:

| Target | Used by | Contains |
|---|---|---|
| `dev` | Dev | nginx + config only. Web app passed through to Vite (`client:5173`). |
| `web` | Prod (default) | nginx + config + **built web app** (`dist/`). No Node, no source. |

## nginx Config Files (`infra/nginx/`)

| File | Used by | Purpose |
|---|---|---|
| `nginx.conf` | Both | HTTP-level: mime types, gzip, upload cap, rate-limit zone, API upstream |
| `api-locations.conf` | Both | `/api`, `/scalar`, `/swagger` → `api` |
| `site.conf` | Prod | Serves built app; unknown paths → `index.html` (client-side routing) |
| `site.dev.conf` | Dev | Everything else → Vite (with WebSocket headers for HMR) |
| `spa-headers.conf` | Prod | CSP + security headers (web app only, never `/api`) |

## Key Details

- `VITE_API_URL` baked at build time (`ARG VITE_API_URL=/api`). API on same origin → CSP can use `connect-src 'self'`.
- Security headers not set globally — API sends its own; strict CSP would block Scalar docs.
- Dev: nginx resolves `client:5173` at startup; fails if not found. Dev override makes `nginx` wait for `client`.
- **TODO:** Topology diagram shows `client` in prod — needs redrawing.