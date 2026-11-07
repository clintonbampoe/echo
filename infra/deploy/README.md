# Deploy scripts

| Script | Runs | What it does |
|---|---|---|
| `server-entry.sh` | on the server, as the deploy key's forced command | accepts `upload <version>`, `deploy <version>`, `rollback` and nothing else |
| `deploy.sh` | on the server, from each release folder | pulls, migrates, starts the release, rolls back if it is not healthy |
| `make-env.sh` | on the server, once | writes `/srv/echo/.env` with fresh secrets |

## First-time server `.env`

Copy the script to the server and run it as the deploy user. It asks for the
domain, an email for certificate notices, the Resend sender address and the
Resend API key (hidden), shows a summary, and writes `/srv/echo/.env` once you
confirm:

```bash
scp infra/deploy/make-env.sh root@<server>:/root/
ssh -t root@<server> 'sudo -u deploy bash /root/make-env.sh; rm /root/make-env.sh'
```

To skip the questions, pass the values as flags; with all four it writes
straight away:

```bash
ssh root@<server> 'sudo -u deploy bash /root/make-env.sh \
  --domain example.com --email you@example.com \
  --mail-from noreply@example.com --resend-key <resend api key>'
```

`--yes` skips the confirmation when some values are still asked for, `--out`
changes where it writes, `--help` lists the flags. It refuses to overwrite an
existing file and starts with `CERTBOT_STAGING=1`; set it to `0` once the
staging certificate works. Everything except those four values is random.

Install `server-entry.sh` by hand as `/usr/local/bin/echo-server-entry.sh`,
owned by root, and pin the deploy key to it in `authorized_keys`:

```
command="/usr/local/bin/echo-server-entry.sh",restrict ssh-ed25519 AAAA… echo-deploy
```
