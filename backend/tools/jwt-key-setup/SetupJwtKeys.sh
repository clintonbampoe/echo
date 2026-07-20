#!/usr/bin/env bash
set -euo pipefail

# Generates a personal RSA key pair for local JWT signing and loads it into
# your own dotnet user-secrets store for Echo.Api. Nothing here is committed
# or shared between teammates — everyone who runs this gets their own key.
#
# Usage:
#   ./setup-jwt-keys.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PROJECT="$SCRIPT_DIR/../../src/Echo.Api"

if [ ! -d "$API_PROJECT" ]; then
  echo "Error: could not find Echo.Api at $API_PROJECT" >&2
  echo "If the repo layout has changed, update API_PROJECT in this script." >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: dotnet CLI not found on PATH." >&2
  exit 1
fi

# user-secrets set requires a UserSecretsId in the .csproj — init is a no-op
# if one already exists, so it's safe to always run.
dotnet user-secrets init --project "$API_PROJECT" >/dev/null

EXISTING_KEY="$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null | grep -c '^Jwt:PrivateKey' || true)"
if [ "$EXISTING_KEY" -gt 0 ]; then
  read -r -p "Jwt:PrivateKey is already set for this project. Overwrite with a new key? [y/N] " REPLY
  if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
    echo "Aborted. Existing key left unchanged."
    exit 0
  fi
fi

WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

cp "$SCRIPT_DIR/GenerateJwtKeys.cs" "$WORK_DIR/"
(cd "$WORK_DIR" && dotnet run GenerateJwtKeys.cs)

dotnet user-secrets set "Jwt:PrivateKey" "$(cat "$WORK_DIR/private.pem")" --project "$API_PROJECT" >/dev/null
dotnet user-secrets set "Jwt:PublicKey" "$(cat "$WORK_DIR/public.pem")" --project "$API_PROJECT" >/dev/null

echo "Done. Jwt:PrivateKey and Jwt:PublicKey are set in your local user-secrets."
echo "Nothing was written to disk outside a temp directory, which has been removed."
