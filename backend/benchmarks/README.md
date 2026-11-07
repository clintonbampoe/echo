# Benchmarks

Measurements that don't belong in the test suite. Run them on the same machine, in the same power profile, every time.

## PasswordHashing

Bcrypt cost 10 (what the API uses) against Argon2id (libsodium, native) and the managed Argon2id library.

Pin it to the two cores the API gets and use the **balanced** power profile, so the numbers match the load tests
(`powerprofilesctl set balanced`; check with `cat /sys/firmware/acpi/platform_profile`). The `quiet` profile ran about 20% slower.

```
cd backend/benchmarks/PasswordHashing

# One hash at a time (BenchmarkDotNet): the cost of one hash
taskset -c 0,1 dotnet run -c Release -- --filter '*'

# Hashes per second with 1, 2 and 4 workers, CPU time per hash and memory
taskset -c 0,1 dotnet run -c Release -- concurrency 10
```

`libsodium` must be installed (`libsodium` on Fedora, `libsodium23` on Debian).

### Results (2 cores, balanced, turbo off, 1.8 GHz)

Concurrency, `concurrency 10`:

| Algorithm | 1 worker | 2 workers | 4 workers | CPU per hash | Peak memory (2 workers) |
|---|---|---|---|---|---|
| bcrypt cost 10 | 6.2/s | 12.6/s | 12.5/s | 154-167 ms | 1 MiB |
| argon2id 19 MiB, t=2 | 39.2/s | 75.9/s | 72.9/s | 25 ms | 38 MiB |
| argon2id 12 MiB, t=3 | 53.0/s | 95.9/s | 91.2/s | 19-21 ms | 24 MiB |
| argon2id 46 MiB, t=1 | 21.7/s | 44.5/s | 42.7/s | 43-46 ms | 92 MiB |

The three Argon2id settings are the OWASP password cheat sheet options it rates as equal in strength. libsodium fixes
parallelism at 1. The managed library (Konscious) is about 4x slower than native at the same settings, so the native
library is what matters.
