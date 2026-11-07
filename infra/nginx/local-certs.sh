#!/usr/bin/env bash
# Installs a mkcert certificate to run the production stack on this machine: local-certs.sh [name]
set -euo pipefail
app=${1:-echo.localhost}
command -v mkcert >/dev/null || {
  echo "Install mkcert first (Fedora: sudo dnf install mkcert nss-tools)" >&2
  exit 1
}

repo=$(cd "$(dirname "$0")/../.." && pwd)
certs=$(mktemp -d)
trap 'rm -rf "$certs"' EXIT
mkcert -install # adds mkcert's own certificate authority to this machine's trust stores, once
mkcert -cert-file "$certs/fullchain.pem" -key-file "$certs/privkey.pem" "$app"

cd "$repo"
APP_DOMAIN=$app docker compose -f docker-compose.yml -f docker-compose.prod.yml \
  run --rm --no-deps -v "$certs:/src:ro,z" --entrypoint sh certbot -c \
  "mkdir -p /etc/letsencrypt/live/$app && cp /src/*.pem /etc/letsencrypt/live/$app/"
echo "Installed the certificate for $app."
