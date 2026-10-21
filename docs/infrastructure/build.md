# Build Pipeline

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

![Dockerfile stage graph](./diagrams/DockerfilePipeline.excalidraw.svg)

## Four Stages

| Stage | What happens | Rebuild trigger |
|---|---|---|
| **Restore** | Download NuGet packages | Project/package files change |
| **Compile** | Build with .NET SDK | Source code changes |
| **Publish** | Package app for runtime | After compile |
| **Runtime** | Slim ASP.NET runtime image | Publish output changes |

Dev pipeline stops at **Compile**. Prod continues through **Runtime** — only the slim runtime and compiled app ship.

## Dockerfile Locations

- `backend/src/Echo.Api/Dockerfile` — API image (multi-stage)
- `infra/nginx/Dockerfile` — nginx image (two targets: `dev`, `web`)
- `backend/src/Echo.MigrationService/Dockerfile` — migrator
- `backend/src/Echo.Seeder/Dockerfile` — seeder
- `infra/postgres/backup/Dockerfile` — backup