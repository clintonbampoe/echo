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