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
  `echo_<timestamp>.dump`. The backup container sees that folder as
  `/app/dumps`. The run's output is appended to `backup.log` in the same
  folder.
- **Kept:** dumps older than 30 days are deleted.
- **Not off the server.** The dumps live on the same disk as the database. A
  lost server loses both. Copying them elsewhere is not set up yet; see
  [Limits](../explanation/limits.md).

`BACKUP_DIR` must point outside the release folders, or the dumps are deleted
with the release. You do not have to set it: unless the server's `.env` sets
one, each deploy uses `/srv/echo/data/dumps` and records it in the release's
`release.env`. Set it in `.env` only to keep dumps somewhere else; the deploy
refuses a relative path or one inside the releases folder.

A server that ran releases before this change keeps writing dumps into its old
release folder until the next release deploys. Copy anything you want to keep
from `/srv/echo/releases/<version>/infra/postgres/backup/dumps/` into
`/srv/echo/data/dumps/` afterwards.

## Run compose on the server

The commands below use a small helper that runs compose against the current
release, as the deploy user. Paste it into a root shell on the server (it is
gone when you log out). `release.env` is in `current/`, written by the deploy;
it holds the image names:

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

This runs the script directly, so it does not report to the status page; only
the nightly run does.

```text
[2026-11-07T15-20-01] Starting backup of echo_db
[2026-11-07T15-20-01] Written: /app/dumps/echo_2026-11-07T15-20-01.dump (3.1M)
[2026-11-07T15-20-01] Pruned dumps older than 30 days
[2026-11-07T15-20-01] Backups complete.
```

On your machine in development, `docker compose --profile db-tools run --rm
backup /app/backup.sh` writes to `infra/postgres/backup/dumps/`.

## Restore

Restoring replaces the database's contents with the dump's, so first take a
fresh dump of the current state (above) in case you need to undo it. To restore
a dump from somewhere else, copy it into `BACKUP_DIR` first. List what is
there:

```bash
ls -lh /srv/echo/data/dumps
```

Stop the API so nothing writes while the restore runs. nginx stays up and
answers `502` for `/api` until the API is back.

```bash
dc stop api
dc exec backup /app/restore.sh /app/dumps/echo_2026-11-07T02-00-01.dump
dc start api
```

```text
[2026-11-07T15:30:00Z] Restoring echo_db from /app/dumps/echo_2026-11-07T02-00-01.dump
[2026-11-07T15:30:09Z] Restore complete.
```

`restore.sh` connects as `DB_USERNAME` from the server's `.env` and runs
`pg_restore --clean --if-exists`: it drops each object before recreating it, so
the database does not need to be empty. Afterwards check
`https://<domain>/api/health/ready`: `200` means it is healthy.

Restoring an older dump goes back in time for every church's data. If the
current release has applied migrations the dump does not have, the API reports
pending migrations on `/api/health/ready`; run the migrator
([Migrations](./migrations.md)).
