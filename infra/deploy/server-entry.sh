#!/usr/bin/env bash
# The only command the deploy SSH key may run: upload <version> | deploy <version> | rollback.
set -euo pipefail

ROOT=${ECHO_ROOT:-/srv/echo}

read -r action version extra <<< "${SSH_ORIGINAL_COMMAND:-}"
[[ -z ${extra:-} ]] || { echo "unexpected arguments" >&2; exit 2; }

case "$action" in
  upload | deploy)
    [[ ${version:-} =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]] || {
      echo "a release is named by its version, like v0.1.0" >&2
      exit 2
    }
    dir="$ROOT/releases/$version"
    ;;
  rollback) ;;
  *) echo "usage: upload <version> | deploy <version> | rollback" >&2; exit 2 ;;
esac

case "$action" in
  upload)
    if [[ -L $ROOT/current && $(basename "$(readlink "$ROOT/current")") == "$version" ]]; then
      echo "$version is the running release" >&2
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
