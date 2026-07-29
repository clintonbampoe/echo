#!/usr/bin/env bash
set -euo pipefail

# Generates a personal RSA key pair (base64-encoded) for local JWT signing.
# Target "user-secrets" (default) loads it into dotnet user-secrets for Echo.Api.
# Target "env" appends it to the repo-root .env file for Docker Compose.
#
# Usage:
#   ./setup-jwt-keys.sh                # user-secrets (default)
#   ./setup-jwt-keys.sh env            # .env for Docker

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PROJECT="$SCRIPT_DIR/../../src/Echo.Api"
ENV_FILE="$SCRIPT_DIR/../../../.env"
TARGET="${1:-user-secrets}"

if [[ "$TARGET" != "user-secrets" && "$TARGET" != "env" ]]; then
  echo "Usage: $0 [user-secrets|env]" >&2
  exit 1
fi

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

  EXISTING_KEY="$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null | grep -c '^Jwt:PrivateKey' || true)"
  if [ "$EXISTING_KEY" -gt 0 ]; then
    read -r -p "Jwt:PrivateKey is already set. Overwrite with a new key? [y/N] " REPLY
    if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
      echo "Aborted. Existing key left unchanged."
      exit 0
    fi
  fi

  dotnet user-secrets set "Jwt:PrivateKey" "$PRIVATE_B64" --project "$API_PROJECT" >/dev/null
  dotnet user-secrets set "Jwt:PublicKey" "$PUBLIC_B64" --project "$API_PROJECT" >/dev/null
  echo "Done. Jwt:PrivateKey and Jwt:PublicKey (base64) set in local user-secrets."
else
  touch "$ENV_FILE"
  if grep -q '^JWT_PRIVATE_KEY=' "$ENV_FILE" 2>/dev/null; then
    read -r -p "JWT_PRIVATE_KEY already exists in .env. Overwrite with a new key? [y/N] " REPLY
    if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
      echo "Aborted. Existing .env left unchanged."
      exit 0
    fi
  fi

  grep -v '^JWT_PRIVATE_KEY=\|^JWT_PUBLIC_KEY=' "$ENV_FILE" >"${ENV_FILE}.tmp" 2>/dev/null || true
  mv "${ENV_FILE}.tmp" "$ENV_FILE"
  {
    echo "JWT_PRIVATE_KEY=$PRIVATE_B64"
    echo "JWT_PUBLIC_KEY=$PUBLIC_B64"
  } >>"$ENV_FILE"
  echo "Done. JWT_PRIVATE_KEY and JWT_PUBLIC_KEY (base64) written to .env."
fi
