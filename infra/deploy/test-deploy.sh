#!/usr/bin/env bash
# Runs deploy.sh against a fake docker and curl and checks how it handles BACKUP_DIR.
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
root=$(mktemp -d)
trap 'rm -rf "$root"' EXIT
mkdir "$root/bin"
cat > "$root/bin/docker" <<'EOF'
#!/bin/sh
echo "${BACKUP_DIR-unset}" >> "$LOG"
case "$*" in *"inspect api"*) echo "ghcr.io/x/echo-api:$TAG" ;; esac
EOF
printf '#!/bin/sh\n' > "$root/bin/curl"
printf '#!/bin/sh\n' > "$root/certs.sh"
chmod +x "$root/bin/docker" "$root/bin/curl" "$root/certs.sh"

release() {
  mkdir -p "$root/releases/$1"
  cp "$here/deploy.sh" "$root/releases/$1/"
  cp "$root/certs.sh" "$root/releases/$1/"
  : > "$root/releases/$1/docker-compose.yml"
  : > "$root/releases/$1/docker-compose.prod.yml"
  printf 'IMAGE_TAG=%s\n' "$1" > "$root/releases/$1/release.env"
}
deploy() { # version, then .env lines
  local version=$1
  shift
  printf '%s\n' RunMigrationsOnStartup=false APP_DOMAIN=example.com "$@" > "$root/.env"
  rm -f "$root/current" "$root/previous"
  : > "$root/log"
  release "$version"
  LOG=$root/log TAG=$version PATH=$root/bin:$PATH bash "$root/releases/$version/deploy.sh" > /dev/null 2>&1
}
fail() { echo "$1" >&2; exit 1; }

deploy v1 || fail "a .env without BACKUP_DIR should deploy"
grep -qx "BACKUP_DIR=$root/data/dumps" "$root/releases/v1/release.env" ||
  fail "the default was not recorded in release.env"
grep -qvx "$root/data/dumps" "$root/log" && fail "compose did not see the default"

deploy v2 BACKUP_DIR=/mnt/backups || fail "an absolute BACKUP_DIR should deploy"
grep -q BACKUP_DIR "$root/releases/v2/release.env" && fail "a set BACKUP_DIR was overwritten"
grep -qvx /mnt/backups "$root/log" && fail "compose did not see the configured path"

deploy v3 BACKUP_DIR=dumps && fail "a relative BACKUP_DIR should be refused"
deploy v4 "BACKUP_DIR=$root/releases/v4/dumps" && fail "a BACKUP_DIR inside a release should be refused"
[[ ! -s $root/log ]] || fail "docker ran before the BACKUP_DIR check"

printf '%s\n' RunMigrationsOnStartup=false APP_DOMAIN=example.com > "$root/.env"
release v5
release v6
ln -sfn "$root/releases/v6" "$root/current"
ln -sfn "$root/releases/v5" "$root/previous"
LOG=$root/log TAG=v5 PATH=$root/bin:$PATH bash "$root/releases/v6/deploy.sh" rollback > /dev/null 2>&1
grep -qx "BACKUP_DIR=$root/data/dumps" "$root/releases/v5/release.env" ||
  fail "a rollback target did not get BACKUP_DIR"
# Removing old releases is housekeeping after a deploy that worked. An old release may hold files the deploy user
# cannot remove (the backup container wrote its dumps there, as root, before BACKUP_DIR moved out of the releases):
# that must not fail the deploy. The oldest release is the last one removed, which is where it used to show.
# A root user can remove anything, so there is nothing to check.
if ((EUID != 0)); then
  printf '%s\n' RunMigrationsOnStartup=false APP_DOMAIN=example.com > "$root/.env"
  rm -f "$root/current" "$root/previous"
  for n in 10 11 12 13 14 15; do release "v$n"; done
  mkdir "$root/releases/v10/dumps"
  : > "$root/releases/v10/dumps/echo.dump"
  chmod a-w "$root/releases/v10/dumps"
  for n in 10 11 12 13 14 15; do touch -d "2020-01-$n" "$root/releases/v$n"; done # last: adding files moves the date
  release v16
  LOG=$root/log TAG=v16 PATH=$root/bin:$PATH bash "$root/releases/v16/deploy.sh" > "$root/out" 2>&1 ||
    fail "an old release that cannot be removed should not fail the deploy"
  grep -q "could not remove the old release v10" "$root/out" || fail "the release left behind was not reported"
  [[ ! -e $root/releases/v11 ]] || fail "an old release that could be removed was kept"
  [[ -e $root/releases/v10 ]] || fail "a release was removed that holds files we could not remove"
  grep -q "Done" "$root/out" || fail "the deploy did not finish"
  chmod -R u+w "$root/releases/v10"
fi
echo "deploy.sh ok"
