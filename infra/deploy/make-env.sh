#!/usr/bin/env bash
# Writes the server's .env with fresh secrets: make-env.sh [--help]
set -euo pipefail

usage() {
  cat <<'USAGE'
usage: make-env.sh [--domain D] [--email E] [--mail-from M]
                   [--resend-key K] [--out FILE] [--yes]

Asks for anything not given as a flag. With all four values given, or
--yes, it writes straight away instead of asking to confirm.
  --domain      the name the app is served on, e.g. example.com
  --email       where Let's Encrypt sends certificate notices
  --mail-from   sender address verified in Resend
  --resend-key  Resend API key
  --out         where to write (default /srv/echo/.env)
USAGE
}

DOMAIN='' EMAIL='' MAIL_FROM='' RESEND_KEY='' OUT=/srv/echo/.env YES=''
while (($#)); do
  case $1 in
    --domain)
      DOMAIN=${2:?--domain needs a value}
      shift 2
      ;;
    --email)
      EMAIL=${2:?--email needs a value}
      shift 2
      ;;
    --mail-from)
      MAIL_FROM=${2:?--mail-from needs a value}
      shift 2
      ;;
    --resend-key)
      RESEND_KEY=${2:?--resend-key needs a value}
      shift 2
      ;;
    --out)
      OUT=${2:?--out needs a value}
      shift 2
      ;;
    --yes | -y)
      YES=1
      shift
      ;;
    --help | -h)
      usage
      exit 0
      ;;
    *)
      usage >&2
      exit 2
      ;;
  esac
done

[[ ! -e $OUT ]] || {
  echo "$OUT already exists; remove it first." >&2
  exit 1
}

domain_re='^([a-z0-9-]+\.)+[a-z]{2,}$'
email_re='^[^@ ]+@[^@ ]+\.[^@ ]+$'
key_re='^[^ ]+$'
asked=''

ask() {
  local var=$1 label=$2 re=$3 secret=${4:-} reply
  while [[ ! ${!var} =~ $re ]]; do
    if [[ -n ${!var} ]]; then
      echo "  not valid: $label" >&2
      printf -v "$var" ''
    fi
    [[ -t 0 ]] || {
      echo "missing or invalid: $label (see --help)" >&2
      exit 2
    }
    asked=1
    if [[ -n $secret ]]; then
      read -rsp "$label: " reply
      echo
    else read -rp "$label: " reply; fi
    printf -v "$var" '%s' "$reply"
  done
}

ask DOMAIN "Domain the app is served on (e.g. example.com)" "$domain_re"
ask EMAIL "Email for certificate notices" "$email_re"
ask MAIL_FROM "Sender address verified in Resend" "$email_re"
ask RESEND_KEY "Resend API key (hidden)" "$key_re" secret

if [[ -n $asked && -z $YES ]]; then
  cat <<SUMMARY

  domain      $DOMAIN  (and status.$DOMAIN)
  email       $EMAIL
  mail from   $MAIL_FROM
  Resend key  ${RESEND_KEY:0:3}...${RESEND_KEY: -4}
  writes      $OUT
SUMMARY
  read -rp "Write it? [y/N] " go
  [[ $go == [yY]* ]] || {
    echo "Nothing written."
    exit 1
  }
fi

hex() { openssl rand -hex "$1"; }
t=$(mktemp -d)
trap 'rm -rf "$t"' EXIT
openssl genrsa -out "$t/k" 2048 2>/dev/null
priv=$(base64 -w0 "$t/k")
pub=$(openssl rsa -in "$t/k" -pubout 2>/dev/null | base64 -w0)
db=$(hex 16)
dumps=$(dirname "$OUT")/data/dumps
mkdir -p "$dumps"
umask 077
cat >"$OUT" <<ENV
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
BACKUP_DIR=$dumps
ENV
echo "Wrote $OUT (fresh secrets, mode 600, CERTBOT_STAGING=1)."
echo "Next: create the DNS records for $DOMAIN and status.$DOMAIN."
