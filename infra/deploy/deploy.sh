#!/usr/bin/env bash
# Deploys the release folder this script is in, and goes back to the previous
# release if it is not healthy. `deploy.sh rollback` goes back by hand.
set -euo pipefail

NEW=$(cd "$(dirname "$0")" && pwd -P)
ROOT=$(dirname "$(dirname "$NEW")")
core=(db api nginx backup certbot)
monitoring=(alloy loki prometheus tempo grafana status)

log() { echo "[$(date +%T)] $*"; }
die() {
  log "ERROR: $*" >&2
  exit 1
}

app() {
  local dir=$1
  shift
  docker compose -p echo --env-file "$ROOT/.env" --env-file "$dir/release.env" \
    -f "$dir/docker-compose.yml" -f "$dir/docker-compose.prod.yml" "$@"
}

env_value() { grep -E "^$1=" "$ROOT/.env" | tail -n 1 | cut -d= -f2- | tr -d '"'; }
release_of() { [[ -L $ROOT/$1 ]] && basename "$(readlink "$ROOT/$1")" || true; }
point() {
  ln -sfn "$ROOT/releases/$2" "$ROOT/$1.tmp" && mv -T "$ROOT/$1.tmp" "$ROOT/$1"
}

activate() {
  local dir=$1 tag domain
  [[ -z $default_backup_dir ]] || grep -q '^BACKUP_DIR=' "$dir/release.env" ||
    echo "BACKUP_DIR=$BACKUP_DIR" >> "$dir/release.env"
  tag=$(grep -E '^IMAGE_TAG=' "$dir/release.env" | cut -d= -f2-)
  domain=$(env_value APP_DOMAIN)

  app "$dir" up -d --no-build --wait --wait-timeout 180 --remove-orphans \
    "${core[@]}" || return 1
  app "$dir" up -d --no-build "${monitoring[@]}" ||
    log "the monitoring services did not start"
  [[ $(docker inspect api --format '{{.Config.Image}}') == *":$tag" ]] || {
    log "api is not on $tag"
    return 1
  }

  local url="https://$domain"
  local probe=(curl -fsSk --max-time 5 -o /dev/null
    --resolve "$domain:443:127.0.0.1")
  for _ in $(seq 10); do
    "${probe[@]}" "$url/api/health/ready" && "${probe[@]}" "$url/" && return 0
    sleep 3
  done
  log "$domain did not answer"
  return 1
}

rollback() {
  local to from
  to=$(release_of previous)
  from=$(release_of current)
  [[ -n $to ]] || die "there is no previous release"
  log "Rolling back from $from to $to"
  point current "$to"
  point previous "$from"
  activate "$ROOT/releases/$to" || die "$to did not come up either"
  log "Back on $to. The database is not rolled back, so migrations must" \
    "keep the previous release working."
}

exec 9>"$ROOT/.deploy.lock"
flock -n 9 || die "another deploy is running"
[[ -r $ROOT/.env ]] || die "cannot read $ROOT/.env: it must belong to the deploy user"
backup_dir=$(env_value BACKUP_DIR || true)
default_backup_dir=
[[ -n $backup_dir ]] || backup_dir=$ROOT/data/dumps default_backup_dir=1
[[ $backup_dir == /* && $backup_dir != "$ROOT/releases" &&
  $backup_dir != "$ROOT/releases/"* ]] ||
  die "BACKUP_DIR in $ROOT/.env must be an absolute path outside the releases"
export BACKUP_DIR=$backup_dir
[[ ${1:-} == rollback ]] && {
  rollback
  exit
}

[[ $(env_value RunMigrationsOnStartup) == false ]] ||
  die "set RunMigrationsOnStartup=false in $ROOT/.env"
prior=$(release_of current)
before=$(release_of previous)
name=$(basename "$NEW")
[[ $prior != "$name" ]] || die "$name is already running"

log "Deploying $name (was ${prior:-nothing})"
app "$NEW" pull "${core[@]}" migrator "${monitoring[@]}" ||
  die "could not pull the images for $name; nothing was changed"

app "$NEW" up -d --no-build --wait --wait-timeout 180 db
rc=0
app "$NEW" run --rm -T migrator --check=true || rc=$?
if ((rc == 10)); then # the migrator exits 10 when migrations are pending
  log "Migrations are pending: backing up first"
  app "$NEW" run --rm -T --no-deps --entrypoint /app/backup.sh backup ||
    die "the backup failed; nothing was changed"
  app "$NEW" run --rm -T migrator ||
    die "the migration failed; the old release is still running"
elif ((rc != 0)); then
  die "the migration check failed (exit $rc)"
fi

"$NEW/certs.sh" placeholder echo nginx APP_DOMAIN ||
  die "could not create the placeholder certificate"
[[ -n $prior ]] && point previous "$prior"
point current "$name"
if ! activate "$NEW"; then
  [[ -n $prior ]] || {
    rm -f "$ROOT/current"
    die "$name is not healthy and there is nothing to go back to"
  }
  rollback
  if [[ -n $before ]]; then
    point previous "$before"
  else
    rm -f "$ROOT/previous"
  fi
  die "$name failed and was rolled back"
fi

find "$ROOT/releases" -mindepth 1 -maxdepth 1 -printf '%T@ %f\n' |
  sort -rn | tail -n +6 | cut -d' ' -f2 | while read -r old; do
  [[ $old == "$name" || $old == "$prior" ]] || rm -rf "${ROOT:?}/releases/$old"
done
docker image prune -af --filter until=720h >/dev/null
log "Done"
