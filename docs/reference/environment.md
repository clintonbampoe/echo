# Environment variables

Every variable the stack reads. In development they come from `.env` in the
repo root (copy `.env.example`). In production the server's `/srv/echo/.env`
holds them, written once by `infra/deploy/make-env.sh`.

Nested settings use a double underscore: `Jwt:Issuer` is `Jwt__Issuer`.
Environment variables override `appsettings.json`. The API validates its
settings at startup and refuses to boot on a missing or invalid one.

## Database

| Variable | Used by | Purpose |
|---|---|---|
| `DB_NAME` | db | database name |
| `DB_USERNAME` | db, backup | database user; the image creates it on first start |
| `DB_PASSWORD` | db, backup | its password |
| `ConnectionStrings__echodb` | api, migrator, seeder | the connection string; must match the three above: `Host=db;Port=5432;Database=<name>;Username=<user>;Password=<password>` |
| `RunMigrationsOnStartup` | api | `true` migrates on startup (development); `false` in production, where the migrator runs before the API |

## Application

| Variable | Used by | Purpose |
|---|---|---|
| `Frontend__BaseUrl` | api | public app URL, absolute. Used for links in emails. |
| `Api__BaseUrl` | api | the API's public address, `https://<domain>/api` in production |
| `Cors__AllowedOrigins` | api | comma-separated origins allowed to call the API from a browser |
| `Jwt__PrivateKey`, `Jwt__PublicKey` | api | RSA pair, each base64 of a PEM; the public key must match the private one |
| `Jwt__PreviousPublicKey` | api | empty except during a [key rotation](../how-to/rotate-jwt-keys.md) |
| `Jwt__Issuer`, `Jwt__Audience` | api | token issuer and audience |
| `MailClient__Address` | api | sender address; must be on a domain verified in Resend |
| `Resend__ApiKey` | api | Resend API key |
| `Invitations__CodeKey` | api | base64 of 32 random bytes; encrypts invitation codes so the list can show them again. Changing it makes existing codes unreadable (they still work when typed). |

`Frontend__BaseUrl` builds links; `Cors__AllowedOrigins` is a security list.
They are usually the same address but do different jobs.

## HTTPS (production)

| Variable | Used by | Purpose |
|---|---|---|
| `APP_DOMAIN` | nginx, certbot | the name the app is served on. `status.<APP_DOMAIN>` is served too. |
| `CERTBOT_EMAIL` | certbot | where Let's Encrypt sends expiry notices |
| `CERTBOT_STAGING` | certbot | `1` uses Let's Encrypt's test service; `0` for real certificates |

## Observability and status

| Variable | Default | Purpose |
|---|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://alloy:4317` | where the API sends traces, metrics and logs |
| `OTEL_SERVICE_NAME` | `Echo.Api` | service name in telemetry |
| `OTEL_METRIC_EXPORT_INTERVAL` | `15000` | metric export interval, ms |
| `API_LOG_LEVEL` | `Warning` | API log level; `Information` logs every request |
| `GF_ADMIN_PASSWORD` | – | Grafana's admin password, **used only when Grafana first creates its database**. Change it in Grafana afterwards. |
| `MONITOR_PASSWORD` | – | password of the read-only Postgres role `monitor`; created with a new database. Letters and digits only. |
| `STATUS_HEARTBEAT_TOKEN` | – | secret the backup and certificate jobs report to the status page with |
| `ALLOY_HOST` | `echo-app` | names this machine in metrics and logs |
| `API_LOG_MIN_SEVERITY` | `13` | API log severity Alloy keeps (13 = warnings and above) |
| `TRACES_SAMPLE_PERCENT` | `1` | share of ordinary traces kept |
| `LOKI_RETENTION`, `LOKI_NGINX_RETENTION` | `672h`, `168h` | log retention |
| `TEMPO_RETENTION`, `PROMETHEUS_RETENTION` | `168h`, `30d` | trace and metric retention |

## Deploy

| Variable | Purpose |
|---|---|
| `IMAGE_REGISTRY` | `ghcr.io/<owner>`. The deploy writes it to `release.env`; unset in development, where images are built locally. |
| `IMAGE_TAG` | the version being deployed, like `v0.1.2`. Also from `release.env`. |
| `BACKUP_DIR` | host folder for the nightly dumps. Default `infra/postgres/backup/dumps/`. |

## Rate limits

Behind nginx, the API reads the visitor's address from `X-Forwarded-For`.
The limiter counts a signed-in person on their own (600 requests a minute,
search included) and anyone else by address (100 a minute). Endpoints reached
without a token are counted by address, with room for a whole church behind
one connection: sign-in 30 a minute, renewing or revoking a session 120, and
5 for sign-up, password mail, verification, invitation codes and signing out
everywhere.

`ForwardedHeaders__TrustedNetworks__0` sets which proxies may set
`X-Forwarded-For`. The default is loopback and private ranges, which covers
nginx on the compose network.

## Security headers

The API always sends `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`
and `Referrer-Policy: strict-origin-when-cross-origin`. Outside development it
also sends `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'`.
HSTS is sent by nginx, not the API.

## `dotnet ef` at design time

`DesignTimeDbContextFactory` reads the connection string `Default` from
`appsettings.json`, while the runtime reads `ConnectionStrings__echodb`.
Run migrations through the migrator instead:

```bash
docker compose --profile db-tools run --rm migrator
```
