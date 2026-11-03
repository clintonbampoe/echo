#!/usr/bin/env bash
# A certificate your browser trusts, for running the production stack on this machine. Run it from anywhere:
#   infra/nginx/local-certs.sh              (names: echo.localhost and obs.echo.localhost)
#   infra/nginx/local-certs.sh my.localhost (names: my.localhost and obs.my.localhost)
# The first name is the app, and obs.<name> is the observability server, the same shape as domain.net and
# obs.domain.net in production. Browsers and curl send every *.localhost name to this machine, so nothing needs adding
# to /etc/hosts.
# One mkcert certificate covers both names. It is copied into each stack's letsencrypt volume, where certbot would put
# the real one, so nginx reads the same paths as in production. Then set the name in each stack's .env and start it with
# its production file:
#   APP_DOMAIN=echo.localhost                    in .env                (docker-compose.prod.yml)
#   OBS_DOMAIN=obs.echo.localhost                in infra/observability/.env
#   docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
# Both stacks use port 443, so run one at a time.
set -euo pipefail
app=${1:-echo.localhost}
obs=obs.$app
command -v mkcert >/dev/null || { echo "Install mkcert first (Fedora: sudo dnf install mkcert nss-tools)" >&2; exit 1; }

repo=$(cd "$(dirname "$0")/../.." && pwd)
certs=$(mktemp -d)
trap 'rm -rf "$certs"' EXIT
mkcert -install   # adds mkcert's own certificate authority to this machine's trust stores, once
mkcert -cert-file "$certs/fullchain.pem" -key-file "$certs/privkey.pem" "$app" "$obs"

# Runs certbot's container (it mounts the volume) from the stack's folder. The :z lets SELinux share the folder.
install_cert() {
    (cd "$1" && env "$2=$3" docker compose -f docker-compose.yml -f docker-compose.prod.yml \
        run --rm --no-deps -v "$certs:/src:ro,z" --entrypoint sh certbot -c \
        "mkdir -p /etc/letsencrypt/live/$3 && cp /src/*.pem /etc/letsencrypt/live/$3/")
}
install_cert "$repo" APP_DOMAIN "$app"
install_cert "$repo/infra/observability" OBS_DOMAIN "$obs"
echo "Installed the certificate for $app and $obs."
