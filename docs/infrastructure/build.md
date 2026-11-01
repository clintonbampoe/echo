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

## ReadyToRun

The API image is published ReadyToRun: the app's assemblies are compiled to native code at build time, so a fresh
container does not spend its first minute compiling the code that serves requests. The image is built for the
machine that builds it (`linux-x64` or `linux-arm64`, from `TARGETARCH`), so build it where it will run. The
assemblies double in size (Echo.Application.dll 2.0 → 4.4 MB) and the build takes about a minute longer. The runtime
still recompiles hot methods with full optimisation as the app runs (tiered compilation stays on).
