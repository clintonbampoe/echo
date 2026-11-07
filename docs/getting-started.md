# Getting started

Run Echo on your machine: the database, the API, nginx and the web app, all
in Docker. Takes about 15 minutes, most of it the first image build.

You need Docker and Git. For the key scripts, a bash shell (on Windows, Git
Bash or WSL).

## 1. Clone and configure

```bash
git clone https://github.com/clintonbampoe/echo.git
cd echo
cp .env.example .env
```

Fill in `.env`. The defaults in `.env.example` work for the database. Ask a
maintainer for the Resend API key and the sender address. The reference for every
variable is [Environment variables](./reference/environment.md).

Two secrets need generating, because they must be encoded:

```bash
bash backend/tools/jwt-key-setup/SetupJwtKeys.sh env
bash backend/tools/invitation-key-setup/SetupInvitationKey.sh env
```

The first writes an RSA key pair for signing tokens (base64, because PEM keys
contain characters that break `.env` files). The second writes the key that
encrypts invitation codes; the API does not start without it. Both ask before
overwriting an existing key.

## 2. Start it

```bash
docker compose up
```

The first run builds the images and downloads the SDKs, about 5 minutes with
internet access. Later runs reuse them. By default the API migrates the
database when it starts (`RunMigrationsOnStartup=true`).

## 3. Check it

```bash
curl http://localhost:8080/api/health/ready
```

```text
{"status":"Healthy","totalDurationMs":0.94,"checks":[...]}
```

`ready` returns `200` once the database is reachable and migrated. A `503` that
says `Database has N pending migration(s)` means the stack is fine and you have
not migrated yet: run `docker compose run --rm migrator`.

Then open:

| What | Address |
|---|---|
| The web app, through nginx | http://localhost:8080 |
| The web app, straight from Vite | http://localhost:5173 |
| The API | http://localhost:5025, or http://localhost:8080/api through nginx |

Create your first church and admin at http://localhost:8080/signup/church.

## Day to day

Backend, from the repo root:

```bash
dotnet build backend/Echo.slnx
dotnet run --project backend/src/Echo.Api      # reads .env
dotnet test --solution backend/Echo.slnx --filter "Category=Unit"
```

Frontend:

```bash
cd frontend
npm install
cp .env.example .env.local
npm run dev        # http://localhost:5173
npm run check      # lint, typecheck, tests, build: run before pushing
```

If you open the app at `:8080`, add `http://localhost:8080` to
`Cors__AllowedOrigins` in `.env`, or set `VITE_API_URL=/api` in `.env.local`.

Add a migration: [Migrations](./how-to/migrations.md). Fake data for a
realistic database: `backend/src/Echo.Seeder/Seed.md`.

## Next

- [How Echo is put together](./explanation/topology.md)
- [The API](./api/getting-started.md)
- [Backend architecture](./backend/architecture.md) and [frontend](./frontend/README.md)
- Running it like production (HTTPS, limits): [Services](./reference/services.md)
  and `infra/nginx/local-certs.sh`
