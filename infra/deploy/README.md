# Deploy scripts

| Script | Runs | What it does |
|---|---|---|
| `server-entry.sh` | on the server, as the deploy key's forced command | accepts `upload <version>`, `deploy <version>`, `rollback` and nothing else |
| `deploy.sh` | on the server, from each release folder | pulls, migrates, starts the release, rolls back if it is not healthy |
| `make-env.sh` | on the server, once | writes `/srv/echo/.env` with fresh secrets |

## First-time server `.env`

Copy the script to the server and run it as the deploy user:

```bash
scp infra/deploy/make-env.sh root@<server>:/root/
ssh root@<server> 'sudo -u deploy env DOMAIN=example.com EMAIL=you@example.com \
  RESEND_KEY=<resend api key> MAIL_FROM=<verified sender> bash /root/make-env.sh; rm /root/make-env.sh'
```

It refuses to overwrite an existing `/srv/echo/.env`. It starts with
`CERTBOT_STAGING=1`; set it to `0` once the staging certificate works.
Values it does not generate (the Resend key, the domain) come from the four
variables above; everything else is random.

Install `server-entry.sh` by hand as `/usr/local/bin/echo-server-entry.sh`,
owned by root, and pin the deploy key to it in `authorized_keys`:

```
command="/usr/local/bin/echo-server-entry.sh",restrict ssh-ed25519 AAAA… echo-deploy
```
