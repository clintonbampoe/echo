#!/usr/bin/env bash
# HTTPS certificate setup for a stack's nginx: certs.sh placeholder|init PROJECT SERVICE DOMAIN_VAR
# Runs from a release folder (or the current link), e.g. current/certs.sh init echo nginx APP_DOMAIN
set -euo pipefail

usage="usage: certs.sh placeholder|init PROJECT SERVICE DOMAIN_VAR"
action=${1:?$usage}
project=${2:?$usage}
service=${3:?$usage}
domain_var=${4:?$usage}

release=$(cd "$(dirname "$0")" && pwd -P)
root=$(dirname "$(dirname "$release")")

env_value() { grep -E "^$1=" "$root/.env" | tail -n 1 | cut -d= -f2- | tr -d '"' || true; }
domain=$(env_value "$domain_var")
: "${domain:?set $domain_var in $root/.env}"
live=/etc/letsencrypt/live/$domain

compose=(docker compose -p "$project" --env-file "$root/.env" --env-file "$release/release.env"
  -f "$release/docker-compose.yml" -f "$release/docker-compose.prod.yml")
certbot=("${compose[@]}" run --rm -T --no-deps)

case "$action" in
  placeholder)
    "${certbot[@]}" --entrypoint sh certbot -c "[ -f $live/fullchain.pem ] || {
      mkdir -p $live && openssl req -x509 -nodes -newkey rsa:2048 -days 1 -subj /CN=$domain \
        -keyout $live/privkey.pem -out $live/fullchain.pem; }"
    ;;
  init)
    email=$(env_value CERTBOT_EMAIL)
    : "${email:?set CERTBOT_EMAIL in $root/.env}"
    flags=()
    [ "$(env_value CERTBOT_STAGING)" = 1 ] && flags+=(--staging)
    "${certbot[@]}" --entrypoint sh certbot -c "rm -rf $live /etc/letsencrypt/archive/$domain /etc/letsencrypt/renewal/$domain.conf"
    "${certbot[@]}" --entrypoint certbot certbot certonly --webroot -w /var/www/certbot "${flags[@]}" \
      -d "$domain" --email "$email" --agree-tos --no-eff-email
    "${compose[@]}" exec -T "$service" nginx -s reload
    ;;
  *)
    echo "$usage" >&2
    exit 1
    ;;
esac
