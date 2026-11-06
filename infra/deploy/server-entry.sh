#!/usr/bin/env bash
# The only command the deploy SSH key may run: upload <sha> | deploy <sha> | rollback.
set -euo pipefail

ROOT=${ECHO_ROOT:-/srv/echo}

read -r action sha extra <<< "${SSH_ORIGINAL_COMMAND:-}"
[[ -z ${extra:-} ]] || { echo "unexpected arguments" >&2; exit 2; }

case "$action" in
  upload | deploy)
    [[ ${sha:-} =~ ^[0-9a-f]{7,40}$ ]] || { echo "a release is named by its commit" >&2; exit 2; }
    dir="$ROOT/releases/$sha"
    ;;
  rollback) ;;
  *) echo "usage: upload <sha> | deploy <sha> | rollback" >&2; exit 2 ;;
esac

case "$action" in
  upload)
    if [[ -L $ROOT/current && $(basename "$(readlink "$ROOT/current")") == "$sha" ]]; then
      echo "$sha is the running release" >&2
      exit 1
    fi
    rm -rf "${dir:?}"
    mkdir -p "$dir"
    tar -x --no-same-owner -C "$dir" || { rm -rf "$dir"; exit 1; }
    chmod +x "$dir/deploy.sh"
    ;;
  deploy) exec "$dir/deploy.sh" ;;
  rollback) exec "$ROOT/current/deploy.sh" rollback ;;
esac
