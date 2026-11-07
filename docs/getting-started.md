# Getting started

Run Echo on your machine: the database, the API, nginx and the web app, all
in Docker. Takes about 10 minutes, most of it the first image build.

You need:

- Docker with the `docker compose` plugin, and Git;
- the .NET 10 SDK, because the key script runs it;
- a bash shell (on Windows, Git Bash or WSL);
- Node 24 only if you will work on the frontend outside Docker.

## 1. Clone and configure

```bash
git clone https://github.com/clintonbampoe/echo.git
cd echo
cp .env.example .env
```

Edit `.env`. The database values in `.env.example` work as they are. Set
`MailClient__Address` (the sender address) and `Resend__ApiKey`; ask a
maintainer for both. Without real values, email
(verification, invitations, password reset) does not send. Every variable is
in [Environment variables](./reference/environment.md).

Two secrets (`Jwt__PrivateKey`, `Jwt__PublicKey` and `Invitations__CodeKey`) must be generated,
because they are encoded. The `env` argument tells each script to write into
`.env` at the repo root; with no argument it stores the key in `dotnet
user-secrets` instead:

```bash
bash backend/tools/jwt-key-setup/SetupJwtKeys.sh env
bash backend/tools/invitation-key-setup/SetupInvitationKey.sh env
```

The first writes an RSA key pair for signing tokens (base64, because PEM keys
contain characters that break `.env` files). The second writes the key that
encrypts invitation codes; the API does not start without it. If a key already
exists in `.env`, the script asks `Overwrite ...? [y/N]`; answer `n` to keep it.

## 2. Start it

```bash
docker compose up
```

This starts the database, the API, nginx and the Vite dev server for the web
app, using `docker-compose.yml` plus `docker-compose.override.yml`, which
compose loads by itself in development. It stays in the foreground, so use a
second terminal for the next step, or add `-d`.

The first run builds the images and downloads the SDKs, about 5 minutes with
internet access. Later runs reuse them. By default the API migrates the
database when it starts (`RunMigrationsOnStartup=true`); the separate
`migrator` service is only for when you set that to `false`.

## 3. Check it

```bash
curl http://localhost:8080/api/health/ready
```

```text
{"status":"Healthy","totalDurationMs":0.94,"checks":[...]}
```

Wait about a minute after `up` for the API to become healthy. Until then curl
says `connection refused` or nginx answers `502`. `ready` returns `200` once
the database is reachable and migrated. A `503` that says
`Database has N pending migration(s)` means the stack is fine and you have not
migrated yet: run `docker compose run --rm migrator`.

Then open:

| What | Address |
|---|---|
| The web app, through nginx | http://localhost:8080 |
| The web app, straight from Vite | http://localhost:5173 |
| The API | http://localhost:5025, or http://localhost:8080/api through nginx |

Create your first church and admin at http://localhost:8080/signup/church:
the church's name, phone, email and address, then your own name, email and a
password of at least 8 characters. You are signed in straight away. A
verification email is sent, but you can use the app before verifying. There is
no seeded admin unless you run the seeder.

The API reference (Scalar) is at http://localhost:8080/scalar.

## Day to day

Backend, from the repo root. The API outside Docker needs the database
(`docker compose up db`) and `Host=localhost` in `ConnectionStrings__echodb`,
since `db` only resolves inside the compose network:

```bash
dotnet build backend/Echo.slnx
dotnet run --project backend/src/Echo.Api      # reads .env at the repo root
dotnet test --solution backend/Echo.slnx --filter "Category=Unit"
```

The unit tests need no Docker. The integration tests (Testcontainers) do; see
[Backend testing](./backend/testing.md).

Frontend:

```bash
cd frontend
npm install
cp .env.example .env.local
npm run dev        # http://localhost:5173
npm run check      # lint, typecheck, tests, build: run before pushing
```

`npm run dev` and the compose `client` container both use port 5173; run
`docker compose stop client` before starting Vite yourself. Vite at `:5173`
calls the API at the `VITE_API_URL` in `frontend/.env.local`; if you open the
app through nginx at `:8080` instead, add `http://localhost:8080` to
`Cors__AllowedOrigins` in the root `.env`, or set `VITE_API_URL=/api`.

Add a migration: [Migrations](./how-to/migrations.md). Fake data for a
realistic database: `backend/src/Echo.Seeder/Seed.md`.

## Next

- [How Echo is put together](./explanation/topology.md)
- [The API](./api/getting-started.md)
- [Backend architecture](./backend/architecture.md) and [frontend](./frontend/README.md)
- Running it like production on your machine: `infra/nginx/local-certs.sh`
  installs a trusted certificate for `echo.localhost` (it needs `mkcert`); then
  start the stack with both compose files and `APP_DOMAIN=echo.localhost`.
  [Services](./reference/services.md) explains the files.
