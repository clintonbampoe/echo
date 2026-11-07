#!/usr/bin/env bash
# Writes the server's .env with fresh secrets (run once, as the deploy user).
set -euo pipefail
: "${DOMAIN:?}" "${EMAIL:?}" "${RESEND_KEY:?}" "${MAIL_FROM:?}"
OUT=${OUT:-/srv/echo/.env}
[ ! -e "$OUT" ] || { echo "$OUT exists" >&2; exit 1; }
hex() { openssl rand -hex "$1"; }
t=$(mktemp -d)
trap 'rm -rf "$t"' EXIT
openssl genrsa -out "$t/k" 2048 2>/dev/null
priv=$(base64 -w0 "$t/k")
pub=$(openssl rsa -in "$t/k" -pubout 2>/dev/null | base64 -w0)
db=$(hex 16)
umask 077
cat > "$OUT" <<ENV
DB_HOST=db
DB_PORT=5432
DB_NAME=echo_db
DB_USERNAME=echo
DB_PASSWORD=$db
ConnectionStrings__echodb="Host=db;Port=5432;Database=echo_db;Username=echo;Password=$db"
RunMigrationsOnStartup=false
Jwt__Issuer=echo-api
Jwt__Audience=echo-clients
Jwt__PrivateKey=$priv
Jwt__PublicKey=$pub
Cors__AllowedOrigins=https://$DOMAIN
Frontend__BaseUrl=https://$DOMAIN
Api__BaseUrl=https://$DOMAIN/api
MailClient__Address=$MAIL_FROM
Resend__ApiKey=$RESEND_KEY
Invitations__CodeKey=$(openssl rand -base64 32)
APP_DOMAIN=$DOMAIN
CERTBOT_EMAIL=$EMAIL
CERTBOT_STAGING=1
OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_SERVICE_NAME=Echo.Api
GF_ADMIN_PASSWORD=$(hex 16)
MONITOR_PASSWORD=$(hex 24)
STATUS_HEARTBEAT_TOKEN=$(hex 24)
ENV
echo "wrote $OUT"
