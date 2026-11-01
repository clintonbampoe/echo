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

## Resource limits (production)

The production box has 2 vCPU and 4 GB of RAM; the monitoring stack runs on another machine. `docker-compose.prod.yml`
splits the RAM so that no container can push the host into swapping or the kernel's out-of-memory killer. A limit with
`memswap_limit` equal to it means no swap: a container that goes over is restarted instead of slowing the whole box.

| Container | Memory | Why |
|---|---|---|
| `api` | 800 MB | the .NET heap may use 75% of the limit (600 MB); check the peak in `results/*-memory.txt` |
| `db` | 2 GB | 768 MB shared buffers, about 600 MB for connections and maintenance, the rest page cache; `shm_size` 256 MB |
| `alloy` | 256 MB | ships logs and metrics to the monitoring box; set in `infra/alloy/docker-compose.yaml` (it used 170-200 MB at peak) |
| `nginx` | 128 MB | |
| `backup` | 256 MB | only while a dump runs; `cpu_shares` 128 so it yields to the API and database |

That is about 3.5 GB at most, which leaves the operating system and Docker the last 300 MB of the ~3.8 GB the box reports. That is not generous: add a swap file as a safety net. Re-check these against
`results/<run>-memory.txt` after every load test; they are sized from the peak plus headroom, not from a formula.
The API's garbage collector is workstation + concurrent (`<ServerGarbageCollection>` in `Echo.Api.csproj`);
`DOTNET_gcServer=1` switches it for a test without a rebuild.
