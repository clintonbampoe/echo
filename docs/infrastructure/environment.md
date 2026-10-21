# Environment Variables

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Configuration System

Hierarchical: **Environment Variables (`.env`)** override **`appsettings.json`** defaults.
Nested JSON → double underscore: `Database:Name` → `Database__Name`.
Fail-fast: `.ValidateOnStart()` refuses boot on missing/invalid config (e.g., HTTP in prod).

## Required Variables

| Variable | Used by | Purpose |
|---|---|---|
| `ConnectionStrings__echodb` | api, migrator, seeder | Postgres connection string (runtime). Format: `Host=db;Port=5432;Database=echo_db;Username=postgres;Password=...` |
| `Database__Name` | db, api | Postgres database name |
| `Database__Username` | db, api | Postgres username |
| `Database__Password` | db, api | Postgres password |
| `Frontend__BaseUrl` | api | Public app URL (absolute, HTTPS in prod). Builds email links. |
| `Cors__AllowedOrigins` | api | Comma-separated trusted origins for CORS |
| `Jwt__PrivateKey` | api | Signs access tokens (Base64) |
| `Jwt__PublicKey` | api | Verifies tokens (Base64, must match private key) |
| `Jwt__Issuer` | api | Token issuer |
| `Jwt__Audience` | api | Token audience |
| `MailClient__Address` | api | From address for emails |
| `Resend__ApiKey` | api | Resend API key |

## Optional Variables

| Variable | Default | Purpose |
|---|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | (empty) | OpenTelemetry collector endpoint for traces/metrics |
| `OTEL_SERVICE_NAME` | (empty) | Service name for OpenTelemetry |
| `Jwt__PreviousPublicKey` | (empty) | Previous public key during rotation grace period |
| `RunMigrationsOnStartup` | `true` | Run EF migrations on API startup |
| `ForwardedHeaders__TrustedNetworks__0` | loopback + private ranges | CIDR ranges allowed to set `X-Forwarded-For` |
| `Api__BaseUrl` | (empty) | API base URL for problem details |
| `FrontendClient__BaseUrl` | (empty) | Frontend URL for auth link generation |
| `License__LuckyPennyKey` | (empty) | LuckyPenny integration key |

## CORS vs Frontend Identity

**Do not confuse:**
- `Frontend__BaseUrl` — public URL for generating absolute links (emails, invites). Single HTTPS URL.
- `Cors__AllowedOrigins` — security barrier, comma-separated list of trusted origins for browser requests.
- `FrontendClient__BaseUrl` — used by Auth module for link generation (bound to `FrontendOptions`).

## Client IP & Rate Limiting

Behind nginx, API reads real client IP from `X-Forwarded-For`. Rate limiter partitions by that IP.
`ForwardedHeaders__TrustedNetworks` controls which proxies are trusted (defaults: loopback + Docker private ranges).

## Security Headers (API)

Always: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`.
Non-Dev: `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'`.
No HSTS — add in nginx when TLS is configured.

## Design-Time Note

`DesignTimeDbContextFactory` (for `dotnet ef`) reads connection string `"Default"` from `appsettings.json`, while runtime uses `ConnectionStrings__echodb`. This makes `dotnet ef` commands **broken at design-time**. Use the migrator container for reliable migrations:

```bash
docker compose --profile db-tools run --rm migrator
```