#!/usr/bin/env bash
# Checks that server-entry.sh only lets versioned releases through.
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
root=$(mktemp -d)
trap 'rm -rf "$root"' EXIT
run() { SSH_ORIGINAL_COMMAND=$1 ECHO_ROOT=$root bash "$here/server-entry.sh"; }

for bad in "deploy abc1234" "upload v0.1" "upload v0.1.0 x" "deploy ../x" \
  bogus ""; do
  rc=0
  run "$bad" < /dev/null > /dev/null 2>&1 || rc=$?
  [[ $rc == 2 ]] || { echo "'$bad' exited $rc, expected 2" >&2; exit 1; }
done

mkdir "$root/work"
printf '#!/bin/sh\necho deployed\n' > "$root/work/deploy.sh"
tar -C "$root/work" -c deploy.sh | run "upload v0.1.0"
[[ $(run "deploy v0.1.0") == deployed ]] || { echo "deploy failed" >&2; exit 1; }
echo "server-entry.sh ok"
