# Deploy scripts

| Script | Runs | What it does |
|---|---|---|
| `server-entry.sh` | on the server, as the deploy key's forced command | accepts `upload <version>`, `deploy <version>`, `rollback` and nothing else |
| `deploy.sh` | on the server, from each release folder | pulls, migrates, starts the release, rolls back if it is not healthy |
| `make-env.sh` | on the server, once | writes `/srv/echo/.env` with fresh secrets and the dump folder |
| `test-server-entry.sh` | in CI | checks `server-entry.sh` only lets `vX.Y.Z` releases through |
| `test-deploy.sh` | in CI | runs `deploy.sh` against a fake docker and checks how it handles `BACKUP_DIR` |

How they fit together, and the order to set a server up, is in
[Set up a server](../../docs/how-to/set-up-a-server.md). Releasing and rolling
back: [Release](../../docs/how-to/release.md).

## make-env.sh

Asks for the domain, an email for certificate notices, the Resend sender
address and the Resend API key, shows a summary and writes the file. Pass the
values as flags to skip the questions; with all four it writes straight away:

```bash
sudo -u deploy bash make-env.sh --domain example.com --email you@example.com \
  --mail-from accounts@example.com --resend-key <resend api key>
```

`--yes` skips the confirmation when some values are still asked for, `--out`
changes where it writes, `--help` lists the flags. It refuses to overwrite an
existing file and starts with `CERTBOT_STAGING=1`. Run it as the deploy user, from a folder
that user can read (not `/root`), or the deploy cannot read the file.
