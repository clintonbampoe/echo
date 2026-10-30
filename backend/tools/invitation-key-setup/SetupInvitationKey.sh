#!/usr/bin/env bash
set -euo pipefail

# Generates the key that encrypts invitation codes (32 random bytes, base64), so the invitation
# list can show a code again. Target "user-secrets" (default) loads it into dotnet user-secrets
# for Echo.Api. Target "env" writes it to the repo-root .env file for Docker Compose.
#
# Changing the key later makes the codes already stored unreadable (they show as no code in the
# list, though they still work when typed), so the script asks before overwriting one.
#
# Usage:
#   ./SetupInvitationKey.sh               # user-secrets (default)
#   ./SetupInvitationKey.sh env           # .env for Docker

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_PROJECT="$SCRIPT_DIR/../../src/Echo.Api"
ENV_FILE="$SCRIPT_DIR/../../../.env"
TARGET="user-secrets"

for arg in "$@"; do
  case "$arg" in
    user-secrets | env) TARGET="$arg" ;;
    *)
      echo "Usage: $0 [user-secrets|env]" >&2
      exit 1
      ;;
  esac
done

KEY="$(head -c 32 /dev/urandom | base64)"

if [ "$TARGET" = "user-secrets" ]; then
  if [ ! -d "$API_PROJECT" ]; then
    echo "Error: could not find Echo.Api at $API_PROJECT" >&2
    exit 1
  fi

  if ! command -v dotnet >/dev/null 2>&1; then
    echo "Error: dotnet CLI not found on PATH." >&2
    exit 1
  fi

  dotnet user-secrets init --project "$API_PROJECT" >/dev/null

  if dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null | grep -q '^Invitations:CodeKey'; then
    read -r -p "Invitations:CodeKey is already set. Overwrite? Stored codes become unreadable. [y/N] " REPLY
    if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
      echo "Aborted. Existing key left unchanged."
      exit 0
    fi
  fi

  dotnet user-secrets set "Invitations:CodeKey" "$KEY" --project "$API_PROJECT" >/dev/null
  echo "Done. Invitations:CodeKey (base64) set in local user-secrets."
else
  touch "$ENV_FILE"

  if grep -q '^Invitations__CodeKey=.' "$ENV_FILE" 2>/dev/null; then
    read -r -p "Invitations__CodeKey already exists in .env. Overwrite? Stored codes become unreadable. [y/N] " REPLY
    if [[ ! "$REPLY" =~ ^[Yy]$ ]]; then
      echo "Aborted. Existing .env left unchanged."
      exit 0
    fi
  fi

  grep -v '^Invitations__CodeKey=' "$ENV_FILE" >"${ENV_FILE}.tmp" 2>/dev/null || true
  mv "${ENV_FILE}.tmp" "$ENV_FILE"
  echo "Invitations__CodeKey=$KEY" >>"$ENV_FILE"
  echo "Done. Invitations__CodeKey (base64) written to .env."
fi
