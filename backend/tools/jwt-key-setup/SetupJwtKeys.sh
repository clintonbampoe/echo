#!/usr/bin/env bash
set -euo pipefail

# Generates a personal RSA key pair (base64-encoded) for local JWT signing.
# Target "user-secrets" (default) loads it into dotnet user-secrets for Echo.Api.
# Target "env" appends it to the repo-root .env file for Docker Compose.
#
# With --rotate, the public key currently in use is kept as the previous key so tokens signed
# with it keep validating during a rotation grace period. Without it, the old key is dropped
# outright and every token signed with it stops working immediately.
# See the rotation runbook in docs/Infrastructure.md.
#
# Usage:
#   ./SetupJwtKeys.sh                       # user-secrets (default)
#   ./SetupJwtKeys.sh env                   # .env for Docker
#   ./SetupJwtKeys.sh env --rotate          # .env, keeping the old public key for the grace period
#   ./SetupJwtKeys.sh user-secrets --rotate

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PROJECT="$SCRIPT_DIR/../../src/Echo.Api"
ENV_FILE="$SCRIPT_DIR/../../../.env"
TARGET="user-secrets"
ROTATE=false

for arg in "$@"; do
  case "$arg" in
    user-secrets | env) TARGET="$arg" ;;
    --rotate) ROTATE=true ;;
    *)
      echo "Usage: $0 [user-secrets|env] [--rotate]" >&2
      exit 1
      ;;
  esac
done

if [ ! -d "$API_PROJECT" ]; then
  echo "Error: could not find Echo.Api at $API_PROJECT" >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: dotnet CLI not found on PATH." >&2
  exit 1
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT
cp "$SCRIPT_DIR/GenerateJwtKeys.cs" "$WORK_DIR/"
(cd "$WORK_DIR" && dotnet run GenerateJwtKeys.cs)

PRIVATE_B64="$(cat "$WORK_DIR/private.b64")"
PUBLIC_B64="$(cat "$WORK_DIR/public.b64")"

if [ "$TARGET" = "user-secrets" ]; then
  dotnet user-secrets init --project "$API_PROJECT" >/dev/null

  PREVIOUS_PUBLIC=""
  if [ "$ROTATE" = true ]; then
    PREVIOUS_PUBLIC="$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null |
      sed -n 's/^Jwt:PublicKey = //p')"
    if [ -z "$PREVIOUS_PUBLIC" ]; then
      echo "Error: --rotate needs an existing Jwt:PublicKey to carry over, but none is set." >&2
      exit 1
    fi
  else
    EXISTING_KEY="$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null | grep -c '^Jwt:PrivateKey' || true)"
    if [ "$EXISTING_KEY" -gt 0 ]; then
      read -r -p "Jwt:PrivateKey is already set. Overwrite with a new key? [y/N] " REPLY
      if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
        echo "Aborted. Existing key left unchanged."
        exit 0
      fi
    fi
  fi

  dotnet user-secrets set "Jwt:PrivateKey" "$PRIVATE_B64" --project "$API_PROJECT" >/dev/null
  dotnet user-secrets set "Jwt:PublicKey" "$PUBLIC_B64" --project "$API_PROJECT" >/dev/null

  if [ "$ROTATE" = true ]; then
    dotnet user-secrets set "Jwt:PreviousPublicKey" "$PREVIOUS_PUBLIC" --project "$API_PROJECT" >/dev/null
    echo "Rotated. The previous public key is kept as Jwt:PreviousPublicKey."
    echo "Once the grace period is over, run from the repo root:"
    echo "  dotnet user-secrets remove \"Jwt:PreviousPublicKey\" --project backend/src/Echo.Api"
  else
    dotnet user-secrets remove "Jwt:PreviousPublicKey" --project "$API_PROJECT" >/dev/null 2>&1 || true
    echo "Done. Jwt:PrivateKey and Jwt:PublicKey (base64) set in local user-secrets."
  fi
else
  touch "$ENV_FILE"

  PREVIOUS_PUBLIC=""
  if [ "$ROTATE" = true ]; then
    PREVIOUS_PUBLIC="$(sed -n 's/^Jwt__PublicKey=//p' "$ENV_FILE")"
    if [ -z "$PREVIOUS_PUBLIC" ]; then
      echo "Error: --rotate needs an existing Jwt__PublicKey in .env to carry over, but none is set." >&2
      exit 1
    fi
  elif grep -q '^Jwt__PrivateKey=.' "$ENV_FILE" 2>/dev/null; then
    read -r -p "Jwt__PrivateKey already exists in .env. Overwrite with a new key? [y/N] " REPLY
    if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
      echo "Aborted. Existing .env left unchanged."
      exit 0
    fi
  fi

  grep -v '^Jwt__PrivateKey=\|^Jwt__PublicKey=\|^Jwt__PreviousPublicKey=' "$ENV_FILE" >"${ENV_FILE}.tmp" 2>/dev/null || true
  mv "${ENV_FILE}.tmp" "$ENV_FILE"
  {
    echo "Jwt__PrivateKey=$PRIVATE_B64"
    echo "Jwt__PublicKey=$PUBLIC_B64"
    echo "Jwt__PreviousPublicKey=$PREVIOUS_PUBLIC"
  } >>"$ENV_FILE"

  if [ "$ROTATE" = true ]; then
    echo "Rotated. The previous public key is kept as Jwt__PreviousPublicKey in .env."
    echo "Empty that variable and redeploy once the grace period is over."
  else
    echo "Done. Jwt__PrivateKey and Jwt__PublicKey (base64) written to .env."
  fi
fi
