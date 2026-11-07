# Back up and restore the database

The `backup` container dumps the database every night at 02:00 UTC and reports
the run to the [status page](../explanation/status-page.md). The deploy also
takes a dump before applying any pending migration.

## What a backup is

- **Format:** Postgres custom format (`pg_dump -Fc`): compressed, and it allows
  restoring single tables.
- **Check:** after writing, the script runs `pg_restore -l` on the file. A dump
  that cannot be read fails the job. A readable dump is not proof the data is
  right; restore one on a scratch machine now and then.
- **Where:** `BACKUP_DIR` on the host, `/srv/echo/data/dumps` in production, as
  `echo_<timestamp>.dump`. The run's output is appended to `backup.log` in the
  same folder.
- **Kept:** dumps older than 30 days are deleted.
- **Not off the server.** The dumps live on the same disk as the database. A
  lost server loses both. Copying them elsewhere is not set up yet; see
  [Limits](../explanation/limits.md).

`BACKUP_DIR` must point outside the release folder, or the dumps are deleted
with the release. `make-env.sh` sets it. On a server whose `.env` predates
that:

```bash
sudo -u deploy sh -c 'mkdir -p /srv/echo/data/dumps && echo BACKUP_DIR=/srv/echo/data/dumps >> /srv/echo/.env'
```

The running backup container keeps its old mount until the next release, or
until you recreate it with `dc up -d --force-recreate backup` (the helper is
defined [below](#run-compose-on-the-server)).

## Run compose on the server

The commands below use a small helper that runs compose against the current
release, as the deploy user. Paste it into your shell on the server:

```bash
dc() {
  (cd /srv/echo/current && sudo -u deploy docker compose -p echo \
    --env-file /srv/echo/.env --env-file release.env \
    -f docker-compose.yml -f docker-compose.prod.yml --profile db-tools "$@")
}
```

## Take a backup now

```bash
dc exec backup /app/backup.sh
```

```text
[2026-11-07T15-20-01] Starting backup of echo_db
[2026-11-07T15-20-01] Written: /app/dumps/echo_2026-11-07T15-20-01.dump (3.1M)
[2026-11-07T15-20-01] Pruned dumps older than 30 days
[2026-11-07T15-20-01] Backups complete.
```

On your machine in development, `docker compose --profile db-tools run --rm
backup /app/backup.sh` writes to `infra/postgres/backup/dumps/`.

## Restore

Restoring replaces the database's contents with the dump's. Stop the API
first, so nothing writes while it runs:

```bash
dc stop api
dc exec backup /app/restore.sh /app/dumps/echo_2026-11-07T02-00-01.dump
dc start api
```

```text
[2026-11-07T15:30:00Z] Restoring echo_db from /app/dumps/echo_2026-11-07T02-00-01.dump
[2026-11-07T15:30:09Z] Restore complete.
```

`restore.sh` runs `pg_restore --clean --if-exists`: it drops each object before
recreating it. Check the app afterwards at `/api/health/ready`.

Restoring an older dump goes back in time for every church's data. If the
current release has applied migrations the dump does not have, the API reports
pending migrations on `/api/health/ready`; run the migrator
([Migrations](./migrations.md)).
