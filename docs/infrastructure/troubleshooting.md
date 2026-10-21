# Troubleshooting

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

This section is empty because there have been no incidents yet.
Add entries only after you've actually fixed something — the purpose is to document steps taken to fix a real problem, so we don't solve it twice. Not for guessing what might go wrong.

## How to Write an Entry

| Column | What to write |
|---|---|
| Symptom | What you saw, in plain words — specific enough that someone else hitting it would recognize it. |
| First check | Fastest way to confirm it's this problem — usually a `docker compose logs` command. |
| Root cause | One sentence: what was actually wrong. |
| Fix | Exactly what you did to fix it. |
| Date | Date you confirmed the fix worked. |
| Added by | Your GitHub handle. |

**Add an entry if:** it took real time to figure out, or the cause wasn't obvious from the error alone.

**Don't add an entry if:** it was just a missing env var with no mystery, or the error message already told you exactly what was wrong.

---

## Existing Entries

### 1. API Healthcheck Fails on Startup

| Field | Value |
|---|---|
| Symptom | Docker compose fails: `dependency failed to start: container echo-api-1 is unhealthy`. API logs show it started and is listening. |
| First check | `docker compose logs api \| grep -i jwt` |
| Root cause | `curl` installed in Dockerfile build stage, missing in runtime image (`mcr.microsoft.com/dotnet/aspnet`). |
| Fix | Moved `RUN apt-get update && apt-get install -y curl` from build stage to runtime stage in `backend/src/Echo.Api/Dockerfile`. |
| Date | 2026-08-03 |
| Added by | @ebenezerquayson |

### 2. API Healthcheck Fails on Compose Startup

| Field | Value |
|---|---|
| Symptom | Same unhealthy error. API was actually fine — healthchecks hit it while it was still building (10-15s startup). |
| First check | Inspect docker network; confirm API reachable via `echo-network` gateway. |
| Root cause | Healthchecks ran before API was ready. |
| Fix | Added `start_period: 10s` to healthcheck config in `docker-compose.yml`. |
| Date | 2026-08-25 |
| Added by | @clintonbampoe |
