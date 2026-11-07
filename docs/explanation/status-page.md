# Status page

`https://status.<domain>` shows whether each part of Echo is up, and whether
the scheduled jobs ran. It is [Gatus](https://github.com/TwiN/gatus), one small
container, configured by one file: `infra/status/config.yaml`. It has no login:
anyone with the address can see it. It shows names and up or down, nothing
else.

It answers "is it up?". For why, use [Grafana](./observability.md).

## What it checks

Every check is green or red. Red means down.

| Group | Checks | How |
|---|---|---|
| Core | API | `GET /api/health/ready` through nginx must return 200 |
| Core | Website | `GET /` through nginx must return 200 |
| Core | Database | a TCP connection to `db:5432` |
| Core | HTTPS Certificate | the certificate nginx serves must have more than 14 days left |
| Observability | Grafana, Prometheus, Loki, Tempo, Alloy | each one's ready endpoint must return 200 |
| Jobs | Backup, Certificate Renewal | the job reports in; see below |

Services are polled once a minute with a 5 second timeout. Checks that go
through nginx test the same path a visitor takes, so a red API with a green
database points at nginx or the API, not the database.

The page lists names in alphabetical order, with unhealthy checks first.

## How jobs report

A cron job cannot be polled, so it calls the status page when it finishes.
The backup and certbot crontabs wrap each job in `infra/status/heartbeat.sh`:

```text
0 2 * * *  set -o pipefail; /app/heartbeat.sh jobs_backup /app/backup.sh ...
30 3 * * * /app/heartbeat.sh jobs_certificate-renewal certbot renew ...
```

The script runs the command, then posts its result to
`http://status:8080/api/v1/endpoints/<key>/external?success=<true|false>` with
the secret `STATUS_HEARTBEAT_TOKEN`. It exits with the command's own exit code.

A job shows red when:

- it ran and failed (the exit code was not 0), or
- it did not report within 26 hours. The page records
  `heartbeat: no update received within 26h0m0s`. This covers a stopped
  container, a cron that never fired, and a job that hung or was killed. A
  missed 02:00 backup shows red around 04:00 the next day.

A red job stays red until its next successful run. On a new server a job has
no result until its first report, then turns red only if none arrives within
26 hours.

What it cannot see:

- If the report itself cannot reach the status page (the page is down, or the
  token is wrong), a good run looks missed. That is a false red, never a false
  green.
- It checks the job's exit code. A backup that ran but wrote bad data still
  looks green; the backup script verifies that the dump can be read, nothing
  more.

## History

The page keeps 100 results per check and the hourly summaries behind the
uptime percentages (1 hour, 24 hours, 7 days, 30 days) in the `status-data`
volume. Click a card to see them. The bars on a card are the latest results,
roughly the last 50 minutes, not days.

## Change it

- **Add or change a check:** edit `infra/status/config.yaml`. It ships with the
  next release.
- **Add a job:** add an `external-endpoints` entry, then wrap the job's command
  with `heartbeat.sh <key>`. The key is the group and name in lower case,
  joined by `_`, with spaces as `-` (`jobs_backup`). Renaming a group or a
  check changes its key, so change the crontab too.

## Limits

- It runs on the server it watches. If the server is down, so is the page. Use
  an outside check to see the server itself go down.
- There are no alerts. It does not tell you; you have to look.
- No "degraded" state: a check passes or fails.
