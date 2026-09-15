# Infrastructure

**Written by:** @clintonbampoe
**Last updated:** 2026-09-15 by @clintonbampoe

---

This explains how Echo runs: the Docker containers, how they're built, how they
talk to each other, and every environment variable the app needs.

Not covered here: how the back-end's code is organized (see `Architecture.md`,
not written yet) or how to set up locally (see [Setup.md](GettingStarted.md)).

_If the compose files or Dockerfile have changed since
then, this doc may be out of date — check `git log` on them before trusting it,
and update this doc if you're the one making the change. If not flag this to the maintainers for review_

## The containers

![Compose stack topology](./diagrams/ComposeStack.excalidraw.svg)

**`api`** — the backend. In dev, it rebuilds fast because it skips the restore
step on every change. In prod, it runs the final, compiled version. Nginx only
sends it traffic once it's confirmed healthy.

**`client`** — the frontend React application. In dev, it runs Vite's hot-reloading
server directly on port 5173 (bypassing Nginx). In prod, it is a lightweight Nginx
container serving static files, which the edge Nginx proxies to.

**`db`** — Postgres 18. Data is stored in a volume so it survives restarts. In
prod, only `api` can reach it — it's not open to the outside world.

**`nginx`** — sits in front as the edge proxy. In prod, it routes `/api`, `/health`, and `/swagger` to the `api` container, and all other traffic `/` to the `client` container. In dev, it only routes API traffic.

**`migrator`** — runs database migrations. This is used for controlled updates in CI/CD or when the API is not yet online.

**`backup`** — handles scheduled database dumps. It uses Supercronic instead of standard cron to ensure that environment variables are passed correctly to the backup scripts.

### Why there are three compose files

One base file, shared by everything. One file for dev-only settings, which loads automatically — you don't need to type anything extra. One file for prod-only settings, which you have to ask for explicitly by naming it.

- dev

```bash
docker compose up
```

- prod

  ```bash
  docker compose -f docker-compose.yml -f docker-compose,prod.yml build up
  ```

That last part matters: asking for a specific file by name switches off the automatic dev file entirely, so dev settings and prod settings can never both apply at once by accident.

## How the build works

![Dockerfile stage graph](./diagrams/DockerfilePipeline.excalidraw.svg)

Building the app happens in four steps:

- **Restore** — download the packages the app needs. This only reruns when a project or package file actually changes — editing regular source code doesn't  trigger it, which is what makes rebuilds fast.
- **Compile** — Pulls the entire .NET Sdk to build the code. the dev pipeline stops here.
- **Publish** — package the app to run.
- **Runtime** — the final image. Only pulls the slim ASP.NET runtime to run the compiled package. No build tools included, just what's needed to run the app. This is what actually ships to prod.

## Ports

| Port | Service | Reachable from outside?            | Dev | Prod |
| ---- | ------- | ---------------------------------- | --- | ---- |
| 8080 | nginx   | YES                                | YES | —    |
| 80   | nginx   | YES                                | —   | YES  |
| 5173 | client  | YES (dev convenience, skips nginx) | YES | —    |
| 5025 | api     | YES (dev convenience, skips nginx) | YES | —    |
| 5432 | db      | YES in dev, no in prod             | YES | --   |

Requests in production always flow the same way: **edge nginx → api (or client)**. Nothing else is open.

## How data persists

To persist data in the containers, we've mounted certain named volumes that data is saved into outside the containers to survive container teardown

| Volume          | What it's for          | Can I delete it?                                     |
| --------------- | ---------------------- | ---------------------------------------------------- |
| `postgres-data` | The actual database    | No — deleting this deletes your data.                |
| `nuget-cache`   | Speeds up dev rebuilds | YES, safely. It'll just rebuild the cache next time. |
| `backup-dumps`   | Database backup files | No — deleting this removes your backups.             |

## Database Backups

Supercronic is used to schedule database backups.

- **Tooling**: The container uses `postgresql18-client`. The client version must match the server version (Postgres 18) to ensure backup reliability.
- **Format**: Backups are created in the Postgres custom binary format (`-Fc`). This format allows for compressed files and selective restoration.
- **Logging**: The crontab uses `2>&1 | tee -a /app/dumps/backup.log`. This captures both standard output and error messages in both the Docker logs and the log file.
- **Scheduling**: The crontab is copied into the image but is also mounted as a volume. This allows the backup schedule to be changed on the host without rebuilding the image.

## Database Migrations

The database schema is updated through two primary paths:

- **Automatic**: When `RunMigrationsOnStartup` is set to `true`, the API applies pending migrations automatically during startup.
- **Manual**: The `migrator` container can be run independently to apply migrations:
  ```bash
  docker compose run --rm migrator
  ```
- **Local Development**: To manage migrations during development, use the following .NET EF commands:
  ```bash
  dotnet ef migrations add "MigrationName" --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api
  dotnet ef database update --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api
  ```

## Configuration

Echo uses a hierarchical configuration system designed to be explicit and predictable. The application reads settings in a specific order of priority: **Environment Variables (`.env`)** override **`appsettings.json`** defaults.

### The Configuration System

To keep settings organized, the application groups them into categories. To override a nested JSON setting via the `.env` file, we use a **double underscore (`__`)** naming convention. For example, a setting located at `Database:Name` in the JSON is overridden by `Database__Name` in the environment.

This approach allows the API to map flat environment variables directly into structured C# Options classes. To ensure stability, the API employs a **Fail-Fast** principle: using `.ValidateOnStart()`, the app will refuse to boot if a required configuration is missing or invalid (e.g., using HTTP in production). This ensures configuration errors are caught during deployment rather than as runtime failures.

### Variable Manifest

The following table lists all available configuration variables. If you add a new variable to the code, update this table to maintain the source of truth.

| Variable                 | Required | Used by | Purpose                                                                            |
| ------------------------ | -------- | ------- | ---------------------------------------------------------------------------------- |
| `Database__Name`         | YES      | db, api | Name of the Postgres database.                                                     |
| `Database__Username`     | YES      | db, api | Postgres login username.                                                           |
| `Database__Password`     | YES      | db, api | Postgres login password.                                                           |
| `Frontend__BaseUrl`    | YES      | api     | **Identity**: The public URL of the app. Used to build links in outbound emails.   |
| `Cors__AllowedOrigins`   | YES      | api     | **Security**: Comma-separated list of domains allowed to make requests to the API. |
| `Jwt__PrivateKey`        | YES      | api     | Signs login tokens (Base64 encoded).                                               |
| `Jwt__PublicKey`         | YES      | api     | Verifies login tokens (Base64 encoded).                                            |
| `Jwt__Issuer`            | YES      | api     | Token issuer identity.                                                             |
| `Jwt__Audience`          | YES      | api     | Token intended audience.                                                           |
| `MailClient__Address`    | YES      | api     | The "from" address for outbound emails.                                            |
| `MailClient__ApiKey`     | YES      | api     | API key for the Resend email service.                                              |
| `RunMigrationsOnStartup` | NO       | api     | `true` by default. Applies DB updates on boot.                                     |                                                                                  |
| `License__LuckyPennyKey` | NO       | api     | License key for LuckyPenny integration.                                            |

### CORS & Frontend Identity

Because the frontend interacts with the API in two fundamentally different ways, we use two distinct configuration paths. **It is critical not to confuse the two.**

**1. Frontend Identity (`Frontend__PublicUrl`)**
This is the "Public Face" of the application—a single, absolute URL (e.g., `https://app.echo.church`). The API uses this value to generate absolute links for password resets, email verifications, and invitation links. For security, this must be an HTTPS URL in all environments except Development.

**2. CORS Security (`Cors__AllowedOrigins`)**
This is a security barrier—a comma-separated list of trusted origins (e.g., `http://localhost:5173,https://app.echo.church`). It tells the browser which domains are authorized to make requests to the API. If a request comes from an origin not in this list, the API rejects the request and the browser blocks the response. To add a new environment (such as a staging site), simply append the URL to this list.

---

## Troubleshooting

This section is empty cause there have been no incidents yet.
Add to it only after you've actually fixed something —
It's main purpose is to document the steps we took to fix a problem, so that we don't have to solve it twice, not to guess what might go wrong in advance.

### How to write an entry

| Column      | What to write                                                                                   |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| Symptom     | What you saw, in plain words — specific enough that someone else hitting it would recognize it. |
| First check | The fastest way to confirm it's this problem — usually a `docker-compose logs` command.         |
| Root cause  | One sentence: what was actually wrong.                                                          |
| Fix         | Exactly what you did to fix it.                                                                 |
| Date        | The date you confirmed the fix worked.                                                          |
| Added by    | Your GitHub handle, in case someone has questions.                                              |

**Add an entry if:** it took you real time to figure out, or the cause wasn't
obvious from the error alone.

**Don't add an entry if:** it was just a missing env var with no real mystery, or
the error message already told you exactly what was wrong.

See [Conventions in README](./README.md#conventions) for the full ruleset on who can add entries and how.

### Example

| Field       | Value                                                                                                  |
| ----------- | ------------------------------------------------------------------------------------------------------ |
| Symptom     | API keeps restarting right after `docker-compose up`. Logs say `No supported key formats were found`.  |
| First check | `docker-compose logs api \| grep -i jwt`                                                               |
| Root cause  | `JWT_PRIVATE_KEY` in `.env` was raw PEM, not base64. Raw PEM doesn't survive `.env`'s format properly. |
| Fix         | Run `sh backend/tools/jwt-key-setup/setup-jwt-keys.sh env` to regenerate the keys correctly.           |
| Date        | 2026-07-31                                                                                             |
| Added by    | @clintonbampoe                                                                                                                        |

---

### 1. API Healthcheck Fails on Startup

| Field       | Value                                                                                                                                                                        |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Symptom     | Docker compose fails to start the stack with error: `dependency failed to start: container echo-api-1 is unhealthy`. API logs show it has started and is listening properly. |
| First check | Inspect `docker-compose.yml` to see what command the healthcheck is running (usually `curl -f http://localhost:8080/api/health/live`).                                       |
| Root cause  | The `curl` package was installed in the `build` stage of the Dockerfile, but was missing in the final `runtime` image based on `mcr.microsoft.com/dotnet/aspnet`.            |
| Fix         | Moved `RUN apt-get update && apt-get install -y curl` from the `build` stage down to the `runtime` stage in `backend/src/Echo.Api/Dockerfile`.                               |
| Date        | 2026-08-03                                                                                                                                                                   |
| Added by    | @ebenezerquayson                                                                                                                                                             |

---

### 2. API Healthcheck Fails on Compose Startup

| Field       | Value                                                                                                                                                                                                                                                                   |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Symptom     | Docker compose fails to start the stack with error: `dependency failed to start: container echo-api-1 is unhealthy`. API logs show it has started and is listening properly.                                                                                            |
| First check | Inspect the docker network to make sure the api was properly bound to the network. Confirm that the api was reachable from outside through the `echo-network` IP gateway. This proved that the root cause wasn't from the api but a configuration in our docker compose |
| Root cause  | Since api takes about 10-15 seconds on average to startup, all the healthchecks hit the api while it was building. Hence, all the checks failed prematurely and marked the api as unhealthy but the api was completely fine.                                            P|
| Fix         | Added a `start_period: 10s` tag to the yaml config to delay the health checks until the api had completed its build.                                                                                                                                                    |
| Date        | 2026-08-25                                                                                                                                                                                                                                                          |
| Added by    | @clintonbampoe                                                                                                                                                                                                                                                          |
