# nginx

nginx is the only public entry. It serves the built web app, forwards `/api`
to the API, forwards `/grafana/` to Grafana, and forwards
`status.<domain>` to the status page. The config is baked into the image
(`infra/nginx/Dockerfile`), so a change ships with a release.

## Image targets

| Target | Used in | Contains |
|---|---|---|
| `dev` | development | the config only; the web app is proxied to Vite (`client:5173`) |
| `web` | production | the config, the built web app (`dist/`), HTTPS and certificate handling. No Node, no source. |

`VITE_API_URL` is baked in at build time (`/api`). The API is on the same
origin, so the content security policy can use `connect-src 'self'`.

## Files (`infra/nginx/`)

| File | Target | Purpose |
|---|---|---|
| `nginx.conf` | both | http-level settings: mime types, gzip, 5 MB upload cap, rate-limit zone, JSON access log, API upstream |
| `api-locations.conf` | both | `/api`, `/scalar`, `/swagger` to the API |
| `site.dev.conf` | dev | everything else to Vite, with WebSocket headers for hot reload |
| `site.conf.template` | web | the three servers below; `${APP_DOMAIN}` is filled in at start |
| `grafana-locations.conf` | web | `/grafana/` to Grafana |
| `spa-headers.conf` | web | content security policy and security headers, for the web app only |
| `tls.conf` | web | TLS protocols, session cache, HSTS |
| `certs.sh` | server | `placeholder` and `init`, see below |
| `reload-on-renewal.sh` | web | reloads nginx every 12 hours so it picks up renewed certificates |

## Servers in production

| Server | Answers | Does |
|---|---|---|
| port 80 | `<domain>`, `status.<domain>` | serves the certificate challenge at `/.well-known/acme-challenge/`, redirects the rest to HTTPS |
| port 443, `<domain>` | the app | `/api`, `/grafana/`, and the built web app (unknown paths go to `index.html` for client-side routing) |
| port 443, `status.<domain>` | the status page | proxies everything to the Gatus container |

Grafana and the status page are looked up by name on each request. If either
container is down, nginx still starts and the app still works.

## Limits and logging

- `/api` is limited to 50 requests a second per address, with a burst of 200.
  `/scalar` and `/swagger` have a burst of 20.
- Only errors (status 4xx and 5xx) and 1% of other requests are logged, as
  JSON with the request's trace id, so a log line links to its trace.
- The security headers on the web app (content security policy, `nosniff`,
  frame denial, HSTS) are not set globally. The API sends its own, and a
  strict policy would block the API docs.

## Certificates

`certs.sh` takes the compose project, the service to reload and the name of
the variable that holds the domain:

```bash
./certs.sh placeholder echo nginx APP_DOMAIN   # self-signed, valid one day
./certs.sh init echo nginx APP_DOMAIN          # real certificate, then reload
```

`placeholder` runs on every deploy and does nothing if a certificate already
exists; it lets nginx start on a fresh server. `init` removes the current
certificate and requests one for `<domain>` and `status.<domain>` through the
challenge on port 80. It uses Let's Encrypt's test service while
`CERTBOT_STAGING=1`. The `certbot` container renews daily at 03:30 UTC.

On your own machine, `local-certs.sh` installs a trusted certificate with
`mkcert` for `echo.localhost` and `status.echo.localhost`.

## Dev notes

nginx resolves `client:5173` at startup and fails if the name is missing. The
dev override makes `nginx` wait for `client`.
