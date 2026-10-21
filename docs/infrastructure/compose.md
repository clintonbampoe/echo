# Docker Compose

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Three Compose Files

| File | Purpose | Loads |
|---|---|---|
| `docker-compose.yml` | Base (shared) | Always |
| `docker-compose.override.yml` | Dev-only | Automatically (default) |
| `docker-compose.prod.yml` | Prod-only | Explicitly via `-f` |

### Dev
```bash
docker compose up
```

### Prod
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

The explicit `-f docker-compose.prod.yml` **switches off** the dev override entirely, so dev and prod settings can never both apply by accident.

## Key Differences

| Setting | Dev | Prod |
|---|---|---|
| `nginx` target | `dev` (no web app) | `web` (built app inside) |
| `client` container | Present (Vite) | Absent |
| Ports exposed | 8080, 5173, 5025, 5432 | 80 only |
| `nuget-cache` volume | Mounted | Not mounted |