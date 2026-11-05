# Logging

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Log Driver

All services inherit from base compose anchor:

```yaml
x-logging: &default-logging
  driver: json-file
  options:
    max-size: 10m
    max-file: '10'
```

**Cap:** 100 MiB per container (10 × 10 MiB), rotates oldest out. Applies dev + prod.

## What is logged

Only a small share of normal traffic is recorded; everything that went wrong always is.

| Source | Normal requests | Warnings and errors |
|---|---|---|
| API logs | off (`Logging__LogLevel__Default` is `Warning`; set `API_LOG_LEVEL=Information` to see them) | all |
| API traces | 1% of the fast successful ones (`TRACES_SAMPLE_PERCENT` on Alloy) | all of them, whole: 5xx, the 4xx that matter (400, 403, 409, 429) and anything over 100 ms are always kept (tail sampling in Alloy) |
| nginx access log | a random 1%, each line marked `"sampled":1` | all 4xx and 5xx |

The nginx sample is uniform, so it can be scaled back up: a request count is the lines with `sampled="1"` times 100,
and a request-time percentile is taken over those lines only (the Edge and Overview dashboards do both). Slow requests
are not kept on purpose; that would make the sample look slower than the traffic is. Exact request counts and times
come from the API's metrics in Prometheus. Retention in Loki is by level: warnings and errors 28 days, the sampled nginx lines 7 days, traces 7 days, metrics 30 days. Lines written before this change have no `sampled` field, so those panels
read zero for older time ranges.

## Important Notes

- **Rotation loses history.** `docker compose logs` only shows current window. Persistent history is shipped to Loki by Alloy; see [observability.md](observability.md).
- **Existing containers keep old settings.** Log config fixed at container creation. Recreate to pick up changes:
  ```bash
  docker compose up -d --force-recreate
  ```
- **Verify actual config:**
  ```bash
  docker inspect echo-api-1 --format '{{json .HostConfig.LogConfig}}'
  ```
- **New services:** Always add `logging: *default-logging`. Without it, falls back to uncapped logs.

## Log Locations

Docker stores JSON logs at `/var/lib/docker/containers/<id>/` on host.