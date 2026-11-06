#!/usr/bin/env bash
# Deploys the release folder this script is in, and goes back to the previous release if it is not healthy. `deploy.sh rollback` goes back by hand.
set -euo pipefail

NEW=$(cd "$(dirname "$0")" && pwd -P)
ROOT=$(dirname "$(dirname "$NEW")")

log() { echo "[$(date +%T)] $*"; }
die() { log "ERROR: $*" >&2; exit 1; }

obs() {
  local dir=$1; shift
  docker compose -p obs --env-file "$ROOT/.env" --env-file "$dir/release.env" \
    -f "$dir/docker-compose.yml" -f "$dir/docker-compose.prod.yml" "$@"
}

env_value() { grep -E "^$1=" "$ROOT/.env" | tail -n 1 | cut -d= -f2- | tr -d '"'; }
release_of() { [[ -L $ROOT/$1 ]] && basename "$(readlink "$ROOT/$1")" || true; }
point() { ln -sfn "$ROOT/releases/$2" "$ROOT/$1.tmp" && mv -T "$ROOT/$1.tmp" "$ROOT/$1"; }

answers() {
  for _ in $(seq 20); do
    curl -fsSk --max-time 5 -o /dev/null "$@" && return 0
    sleep 3
  done
  log "no answer from $*"
  return 1
}

activate() {
  local dir=$1 tag domain
  tag=$(grep -E '^IMAGE_TAG=' "$dir/release.env" | cut -d= -f2-)
  domain=$(env_value OBS_DOMAIN)

  obs "$dir" up -d --no-build --wait --wait-timeout 180 --remove-orphans || return 1
  [[ $(docker inspect grafana --format '{{.Config.Image}}') == *":$tag" ]] || { log "grafana is not on $tag"; return 1; }
  answers http://127.0.0.1:3100/ready || return 1
  answers http://127.0.0.1:9090/-/ready || return 1
  answers --resolve "$domain:443:127.0.0.1" "https://$domain/grafana/api/health" || return 1
  [[ $(docker inspect tempo --format '{{.State.Running}} {{.RestartCount}}') == "true 0" ]] || { log "tempo keeps restarting"; return 1; }
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
  log "Back on $to. The data volumes are not rolled back."
}

exec 9> "$ROOT/.deploy.lock"
flock -n 9 || die "another deploy is running"
[[ ${1:-} == rollback ]] && { rollback; exit; }

prior=$(release_of current)
before=$(release_of previous)
name=$(basename "$NEW")
[[ $prior != "$name" ]] || die "$name is already running"

log "Deploying $name (was ${prior:-nothing})"
obs "$NEW" pull loki prometheus tempo grafana proxy certbot || die "could not pull the images for $name; nothing was changed"

"$NEW/certs.sh" placeholder obs proxy OBS_DOMAIN || die "could not create the placeholder certificate"
[[ -n $prior ]] && point previous "$prior"
point current "$name"
if ! activate "$NEW"; then
  [[ -n $prior ]] || { rm -f "$ROOT/current"; die "$name is not healthy and there is nothing to go back to"; }
  rollback
  if [[ -n $before ]]; then point previous "$before"; else rm -f "$ROOT/previous"; fi # not the failed release
  die "$name failed and was rolled back"
fi

find "$ROOT/releases" -mindepth 1 -maxdepth 1 -printf '%T@ %f\n' | sort -rn | tail -n +6 | cut -d' ' -f2 | while read -r old; do
  [[ $old == "$name" || $old == "$prior" ]] || rm -rf "${ROOT:?}/releases/$old"
done
docker image prune -f > /dev/null
log "Done"
