# Set up a server

Takes a fresh Ubuntu or Debian server to a running release. You do this once.
Afterwards, [releasing](./release.md) is one command.

Echo runs on a single server: one compose project, one release at a time.
Hetzner with 4 vCPU and 8 GB is what the setup is sized for. See
[Limits](../explanation/limits.md) before choosing a smaller one.

You need the domain (with its DNS under your control), a Resend account with a
verified sending domain, and `gh` logged in on your laptop.

Each step says where to run it: **server** (as root) or **laptop** (in the
repo).

## 1. Install Docker (server)

```bash
apt-get update && apt-get install -y ca-certificates curl
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
echo "deb [signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo $VERSION_CODENAME) stable" > /etc/apt/sources.list.d/docker.list
apt-get update && apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
docker run --rm hello-world | head -3
```

On Debian, replace `linux/ubuntu` with `linux/debian` in both places. The last
command prints `Hello from Docker!`.

## 2. Create the deploy user and folder (server)

```bash
adduser --disabled-password --gecos "" deploy
usermod -aG docker deploy
install -d -o deploy -g deploy -m 700 /home/deploy/.ssh
chown deploy:deploy /home/deploy/.ssh
install -d -o deploy -g deploy /srv/echo
```

The user has no password. Membership of `docker` is root-equivalent, which is
why step 4 restricts its key to one command.

## 3. Install the entry script (laptop, then server)

The script limits what the deploy key can do. It lives outside `/srv/echo`,
owned by root, so the deploy user cannot replace it.

```bash
scp infra/deploy/server-entry.sh root@<server-ip>:/usr/local/bin/echo-server-entry.sh
```

```bash
chown root:root /usr/local/bin/echo-server-entry.sh
chmod 755 /usr/local/bin/echo-server-entry.sh
```

## 4. Create the deploy key (laptop, then server)

```bash
ssh-keygen -t ed25519 -N "" -C echo-deploy -f ~/.ssh/echo_deploy
cat ~/.ssh/echo_deploy.pub
```

Put the public line in the deploy user's `authorized_keys`, with the
restrictions in front of it (server):

```bash
cat > /home/deploy/.ssh/authorized_keys <<'EOF'
command="/usr/local/bin/echo-server-entry.sh",restrict ssh-ed25519 AAAA... echo-deploy
EOF
chown deploy:deploy /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys
```

`command=` makes SSH run only the entry script, whatever the client asks for.
`restrict` turns off terminals and forwarding. A stolen key can upload a
release folder and deploy a version, and nothing else.

Test from the laptop. It must print the script's message and `exit=2`, never
give you a shell:

```bash
ssh -i ~/.ssh/echo_deploy deploy@<server-ip> deploy abc1234; echo "exit=$?"
```

```text
a release is named by its version, like v0.1.0
exit=2
```

## 5. Open the firewall

Use a Hetzner Cloud Firewall rather than `ufw`: Docker's own rules bypass the
host firewall, while a cloud firewall filters before traffic arrives. Allow
inbound TCP 22, 80 and 443, and ICMP. Nothing else needs to be reachable.

## 6. Write the server's .env (laptop)

```bash
scp infra/deploy/make-env.sh root@<server-ip>:/root/
ssh -t root@<server-ip> 'sudo -u deploy bash /root/make-env.sh; rm /root/make-env.sh'
```

It asks for the domain, an email for certificate notices, the sender address
verified in Resend and the Resend API key. Everything else is random. Run it
as the `deploy` user: a file owned by root fails the deploy with
`Permission denied`. `infra/deploy/README.md` lists the flags for a
non-interactive run.

Check it without printing secrets (server):

```bash
ls -l /srv/echo/.env
sudo -u deploy grep -E '^(APP_DOMAIN|CERTBOT_STAGING)=' /srv/echo/.env
```

```text
-rw------- 1 deploy deploy ...
APP_DOMAIN=example.com
CERTBOT_STAGING=1
```

## 7. Point DNS at the server

Create two A records to the server's IPv4 address: `example.com` and
`status.example.com`. Set both to **DNS only** (grey cloud in Cloudflare).
Do not add an AAAA record; nginx listens on IPv4 only.

With the Cloudflare proxy on, visitors get Cloudflare's certificate and nginx
sees Cloudflare's addresses instead of the visitors', so the per-address rate
limits would count everyone together.

## 8. Connect GitHub (laptop)

```bash
gh api -X PUT repos/<owner>/<repo>/environments/production --silent
gh secret set DEPLOY_SSH_KEY --env production < ~/.ssh/echo_deploy
ssh-keyscan -t ed25519 <server-ip> | gh secret set DEPLOY_KNOWN_HOSTS --env production
gh variable set DEPLOY_HOST --env production --body <server-ip>
gh variable set DEPLOY_USER --env production --body deploy
```

Before trusting the host key, compare the fingerprint from
`ssh-keyscan -t ed25519 <server-ip> | ssh-keygen -lf -` with the one from
`ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub` on the server. They must
match. Add required reviewers to the `production` environment if you want to
approve every deploy.

## 9. Let the server pull images

Create a classic GitHub token with only `read:packages`, then log in as the
deploy user (server):

```bash
sudo -u deploy -H docker login ghcr.io -u <github-user>
```

Paste the token at the prompt. It prints `Login Succeeded`. When the token
expires, deploys fail at the pull step.

## 10. First release and certificate

Cut the first release as described in [Release](./release.md). The first
deploy starts nginx with a temporary self-signed certificate, so the site
answers but browsers warn.

Then get the real certificate. `CERTBOT_STAGING=1` sends the first request to
Let's Encrypt's test service, which has no rate limits. Run it from `/srv/echo`
(the deploy user cannot read `/root`):

```bash
cd /srv/echo
sudo -u deploy -H ./current/certs.sh init echo nginx APP_DOMAIN
```

```text
Successfully received certificate.
```

When that works, switch to the real service and run it again:

```bash
sudo -u deploy sed -i 's/^CERTBOT_STAGING=1/CERTBOT_STAGING=0/' /srv/echo/.env
sudo -u deploy -H ./current/certs.sh init echo nginx APP_DOMAIN
```

The certificate covers `example.com` and `status.example.com`. The `certbot`
container renews it daily.

## 11. Check

From anywhere:

```bash
curl -sS -o /dev/null -w '%{http_code}\n' https://example.com/
curl -sS -o /dev/null -w '%{http_code}\n' https://example.com/api/health/ready
curl -sS -o /dev/null -w '%{http_code}\n' https://status.example.com/
```

All three print `200` with no certificate error. Then create the first admin
at `https://example.com/signup/church`.

The Jobs checks on the status page (Backup, Certificate Renewal) turn green
after the first nightly runs, at 02:00 and 03:30 UTC.
