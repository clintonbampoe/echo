#!/usr/bin/env bash
# Gets and renews the HTTPS certificate for a stack's nginx. Used by both servers; run it from the folder that holds the
# stack's .env and its docker-compose.yml + docker-compose.prod.yml:
#
#   app server:  infra/nginx/certs.sh init  nginx APP_DOMAIN
#   obs server:  ../nginx/certs.sh    init  proxy OBS_DOMAIN       (from infra/observability)
#
# `init` is run once. nginx will not start without a certificate, and Let's Encrypt needs nginx running to answer its
# challenge, so it makes a throwaway certificate, starts nginx, swaps in the real one and reloads.
# `renew` renews a certificate within 30 days of expiring and reloads nginx. Run it weekly from the host, for example
#   0 4 * * 1  cd /path/to/stack && /path/to/infra/nginx/certs.sh renew nginx APP_DOMAIN
# Reads CERTBOT_EMAIL, CERTBOT_STAGING (1 = Let's Encrypt's staging service: untrusted certificates, no rate limits) and
# the domain variable from .env.
set -euo pipefail
action=${1:?usage: certs.sh init|renew SERVICE DOMAIN_VAR}
service=${2:?usage: certs.sh init|renew SERVICE DOMAIN_VAR}
domain_var=${3:?usage: certs.sh init|renew SERVICE DOMAIN_VAR}

# Only the three values needed: .env also holds keys and connection strings that are not shell-safe.
env_value() { grep -E "^$1=" .env | tail -1 | cut -d= -f2- | sed -e 's/^"//' -e 's/"$//'; }
domain=$(env_value "$domain_var")
email=$(env_value CERTBOT_EMAIL)
staging=$(env_value CERTBOT_STAGING)
: "${domain:?set $domain_var in .env}"

compose=(docker compose -f docker-compose.yml -f docker-compose.prod.yml)
certbot_flags=(--webroot -w /var/www/certbot)
live="/etc/letsencrypt/live/$domain"

case "$action" in
init)
    : "${email:?set CERTBOT_EMAIL in .env}"
    [ "${staging:-0}" = "1" ] && certbot_flags+=(--staging)
    "${compose[@]}" run --rm --entrypoint sh certbot -c "
        mkdir -p $live &&
        openssl req -x509 -nodes -newkey rsa:2048 -days 1 -subj '/CN=$domain' \
            -keyout $live/privkey.pem -out $live/fullchain.pem"
    "${compose[@]}" up -d --build "$service"
    # Remove the throwaway so certbot can write the real one in its place.
    "${compose[@]}" run --rm --entrypoint sh certbot -c \
        "rm -rf $live /etc/letsencrypt/archive/$domain /etc/letsencrypt/renewal/$domain.conf"
    "${compose[@]}" run --rm certbot certonly "${certbot_flags[@]}" -d "$domain" \
        --email "$email" --agree-tos --no-eff-email
    ;;
renew)
    "${compose[@]}" run --rm certbot renew "${certbot_flags[@]}"
    ;;
*)
    echo "usage: certs.sh init|renew SERVICE DOMAIN_VAR" >&2
    exit 1
    ;;
esac
"${compose[@]}" exec "$service" nginx -s reload
