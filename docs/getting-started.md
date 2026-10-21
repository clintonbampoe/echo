# Getting Started with Echo

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Requirements

- Docker
- Git

---

## 1. Clone the repo

```bash
git clone https://github.com/clintonbampoe/echo.git
```

---

## 2. Configure secrets

- Copy the structure in the `.env.example` file into a new `.env` file. Speak to any of the maintainers of the repository for secrets and API keys.

- Fill in the secrets into the `.env` file and add it to the repository root.

- JWT keys are Base64 encoded. This is because normal PEM keys can have escape characters and other sequences that interfere with the `.env` file format. Hence, the reason for the encoding. Rest assured, every module that reads this key knows to decode it before use. The reader should also keep this in mind for any module they might add that would read the JWT keys.
  **NB:** The encoding is only for the **JWT Keys** due to the quirks of PEM keys. The rest of the secrets are just in their normal form.

- To set up a **JWT Key** pair on your device, navigate to the `/backend/tools/jwt-key-setup` directory in the repository and run:

```bash
sh SetupJwtKeys.sh env
```

  The script checks the `.env` file at the repo root to see if the public and private keys for JWT have been configured. It then asks permission to override if yes, or continues if the key pair has not been generated yet.

  **NB:** On Windows, unless you've configured a bash interpreter (Git Bash, WSL), the script may not run. In that case, contact the maintainer for a **JWT Key Pair**.

---

## 3. Building container images

Echo has a stack that consists of multiple containers (database, nginx, api, etc. See [Infrastructure](../infrastructure/index.md) for more details). Some of these containers run on custom images.

For example, the **api** image is a custom image that runs a multistage build process. Hence, in order to run them you would need to build the image first, so that Docker can cache and reuse it for later sessions.

We have two environments in which our containers can run in:
- **Development**
- **Production**

**Development** is the default environment and it is the image docker builds when you run the command:

```bash
docker compose build
```

This builds our custom docker images for the dev environment.

To build the images for production, you'll need to explicitly pass the production file override `docker-compose.prod.yml` (located at the root of the repository) as a flag during the `compose build`:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml build
```

This pulls all the dependencies needed to build our images and prepares your containers for execution.

> **Note:** On first run, the build might take a while (about 5 minutes depending on internet connectivity). Docker pulls all the SDKs and dependencies needed to run the containers. This only happens once; dependencies and images are cached and reused for later builds.
>
> **THIS IMPLIES THAT YOU'LL NEED INTERNET CONNECTION FOR THE FIRST BUILD**

---

## 4. Starting up containers

To start up the app containers and services, you must have docker installed on your system. Navigate to the repository root to run the following commands.

- **For Development**:

```bash
docker compose up
```

  (no flags)

- **For Production**:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

> **Note:** The `-d` flag runs in detached mode. `--build` ensures fresh images on deploy.

- By default, migrations run automatically when the API container starts (`RunMigrationsOnStartup=true` in `.env`).

- To disable automatic migrations and run them manually instead, set that value to `false` in `.env`, then run:

```bash
docker compose run --rm migrator
```

---

## 5. Local Development Commands

### Backend

```bash
# Build
dotnet build backend/Echo.slnx

# Run API (uses .env at repo root)
dotnet run --project backend/src/Echo.Api

# Run tests
dotnet test backend/Echo.slnx

# EF Core migrations (project = Data, startup = Api)
# Note: DesignTimeDbContextFactory reads "Default" connection string; runtime uses "echodb".
# For reliable migrations, use the migrator container:
#   docker compose --profile db-tools run --rm migrator
dotnet ef migrations add "MigrationName" --project backend/src/Echo.Data --startup-project backend/src/Echo.Api
dotnet ef database update --project backend/src/Echo.Data --startup-project backend/src/Echo.Api
```

### Frontend

```bash
cd frontend
npm install
cp .env.example .env.local
npm run dev       # http://localhost:5173
npm run check     # lint + typecheck + tests + build (run before pushing)
npm test          # Vitest
```

---

## 6. Verify

Once the containers are up, the web app and the API are reachable at:

- **Dev** the web app at `http://localhost:8080` (through Nginx) or `http://localhost:5173` (Vite directly); the API at `http://localhost:5025` (directly) or `http://localhost:8080/api` (through Nginx)
- **Prod** `http://localhost:80` (through Nginx only — Nginx serves the web app, and the API isn't exposed directly)

To confirm it's working, test the health endpoint:

```bash
curl http://localhost:8080/api/health/ready
```

Or open `http://localhost:8080/api/health/ready` in the browser.

`/ready` only returns `200` once the database is reachable **and** fully migrated. If you get a `503` saying `Database has N pending migration(s)`, the stack is fine — you just haven't run the migrations yet, so run the `migrator` container above. See [Health endpoints](../infrastructure/topology.md#health-endpoints).

---

## 7. Related Documentation

- [Infrastructure](../infrastructure/index.md)
- [Backend Architecture](../backend/architecture.md)
- [Echo API](../api/api-usage.md)
- [Front-end](../frontend/README.md)
- [Performance Plan](../performance/plan.md)
- [Performance Log](../performance/log.md)