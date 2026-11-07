# Troubleshooting

Every entry here came from a real incident. The heading is the text you saw, so
search for it. Add an entry when something took real time to work out or the
cause was not obvious from the message. Do not add guesses.

## Deploy

### `cannot read /srv/echo/.env: it must belong to the deploy user`

Also seen, before the message was clearer:
`grep: /srv/echo/.env: Permission denied` followed by
`ERROR: set RunMigrationsOnStartup=false in /srv/echo/.env`.

Cause: the file belongs to root because `make-env.sh` ran as root. The deploy
runs as `deploy` and cannot read it. Running the script as `deploy` from `/root`
fails because that user cannot read `/root`, which is what tempts people to run
it as root; copy the script to `/tmp` first.

Fix, on the server:

```bash
chown deploy:deploy /srv/echo/.env
```

Then run the deploy again for the same version (see
[Release](./how-to/release.md)).

### `BACKUP_DIR in /srv/echo/.env must be an absolute path outside the releases`

Cause: `BACKUP_DIR` in the server's `.env` is relative or points inside the
releases folder. The deploy deletes release folders beyond the newest five, so
dumps kept there would go with them.

Fix: remove the line (the deploy then uses `/srv/echo/data/dumps`) or set an
absolute path outside `/srv/echo/releases`, then run the deploy again.

### `required variable BACKUP_DIR is missing a value`

Seen when running `docker compose` by hand with the production files.

Cause: production compose requires `BACKUP_DIR`. The deploy writes it into the
release's `release.env`; a release that has never been deployed with a current
`deploy.sh` does not have it.

Fix: use the `dc` helper from [Back up and restore](./how-to/restore-a-backup.md)
after a deploy, or `export BACKUP_DIR=/srv/echo/data/dumps` first.

### `password authentication failed for user "echo"`

In the Postgres log: `FATAL:  role "echo" does not exist`, and
`Database directory appears to contain a database; Skipping initialization`
at the start.

Cause: Postgres creates its user only when it starts on an empty volume. The
volume `echo_postgres-data` was first created by an earlier, failed attempt,
so a later start reused it with the wrong roles. (The first attempt had a
second cause, fixed in v0.1.1: the compose file set `POSTGRES_USERNAME`, which
the image ignores. It reads `POSTGRES_USER`.)

Fix, on a server with no data to keep:

```bash
docker rm -f $(docker ps -aq --filter volume=echo_postgres-data)
docker volume rm echo_postgres-data
docker volume ls | grep -i postgres
```

The last command prints nothing. Run the deploy again. If the database holds
data, do not remove the volume; create the missing role instead.

### `error in parsing "compose-spec.json": stat .: permission denied`

Cause: you ran `certs.sh` (or another compose command) as the deploy user from
`/root`, which that user cannot read. Compose looks at the current folder.

Fix: `cd /srv/echo` first.

### Required checks stay "Expected: waiting for status to be reported"

On a pull request the checks `api` and `frontend` never start.

Cause: CI only ran when files under `backend/` or `frontend/` changed, but
branch protection required both checks. A pull request that touched neither
waited forever.

Fix: Echo CI now always starts. A `changes` job decides which jobs run, the
others skip themselves, and a skipped job counts as passing. Never put a
`paths:` filter on a workflow that provides a required check.

## Certificates and DNS

### Browser: "Be careful. Something doesn't look right"

On the root domain right after the first real certificate.

Cause: the DNS record was proxied by Cloudflare (orange cloud), so the browser
was served Cloudflare's certificate and cached DNS for a while after the
switch. The server's own certificate was valid; check it directly:

```bash
echo | openssl s_client -connect <server-ip>:443 -servername example.com 2>&1 | grep 'Verify return'
```

`Verify return code: 0 (ok)` means the server is fine.

Fix: set the record to **DNS only** (grey cloud), wait a few minutes, and
reopen the browser in a private window.

## Nginx

### `[emerg] the duplicate "status" variable`

Cause: a `set $status ...` in a location file clashes with nginx's built-in
`$status`. Nginx refuses to start. Pick a name that is not an nginx variable
(`$gatus` here). CI does not check nginx configs yet; build the nginx image and
run `nginx -t` in it before merging a change to `infra/nginx/`.

## Containers

### `Conflict. The container name "/alloy" is already in use`

Cause: a container from an older compose project (before everything moved to
the single project `echo`) still holds the name.

Fix: `docker rm alloy`, then start the stack again.

### Alloy cannot connect to Postgres: `role "monitor" does not exist`

Cause: the database was created before the `monitor` role was automated, so
the init script never ran on it.

Fix, once, on that database:

```bash
docker exec database /docker-entrypoint-initdb.d/monitor-role.sh
```

New databases create the role on their own from `MONITOR_PASSWORD`.

### `libgssapi_krb5.so.2: cannot open shared object file` in migrator output

Harmless. The migrator image has no Kerberos library and Npgsql logs that it
could not load it. It is not the reason a migration failed; read the next
exception in the log.

## Repository

### GitGuardian flags `set MONITOR_PASSWORD`

Cause: `${MONITOR_PASSWORD:?set MONITOR_PASSWORD}` reads like a password
assignment to the scanner. It is only an error message.

Fix: write `${MONITOR_PASSWORD:?}`. The shell and compose still refuse to
start without the value and print their own message. Because the scan covers
every commit in a pull request, the text has to be removed from the commit that
introduced it.
