# Release, watch and roll back

A release is a version tag on `main`, like `v0.1.2`. The tag builds the
images once and deploys exactly those images to the server. Nothing ships on
an ordinary merge.

Releasing needs the server from [Set up a server](./set-up-a-server.md).

## Cut a release

Run this from inside your clone, as someone who can create releases on the
repository. Make sure CI is green on the commit you want; nothing enforces it.
See the latest version with `gh release list --limit 3`, and pick the next
one (a fix is a new patch number, a feature a new minor).

```bash
gh release create v0.1.2 --target main --title v0.1.2 --generate-notes
```

`--target main` puts the tag on the latest commit of `main`, not on whatever
you have checked out. Version names are `vMAJOR.MINOR.PATCH`; anything else
is rejected by the workflow and by the server.

This also creates a GitHub Release page with generated notes; it is as visible
as the repository. Tags are permanent. Never move or re-push one: its images would be
overwritten under the same name. If a release is bad, cut the next version.

## Watch it

```bash
gh run list --limit 4
```

Two runs appear, in order:

1. **Release** (`release.yaml`) checks the tag is on `main`, then builds the 12
   images in parallel and pushes them to GHCR as `:v0.1.2`. The first build
   with no cache takes 5 to 15 minutes. The image list is in
   [How Echo is put together](../explanation/topology.md).
2. **Deploy** (`deploy.yaml`) starts when Release succeeds. It uploads the release folder to
   the server and runs `deploy.sh` there. It waits for approval if the
   `production` environment has required reviewers.

When it works, the end of the Deploy log says `Done`.

## What deploy.sh does

1. Pulls the images. If the pull fails, nothing has changed.
2. Starts the database and runs `migrator --check`. The migrator is the
   container that applies schema changes ([Migrations](./migrations.md)). If
   migrations are pending (exit code 10) the deploy takes a dump first, then
   migrates. Any other failure stops the deploy before anything changes.
3. Points `current` at the new release and starts `db`, `api`, `nginx`,
   `backup` and `certbot`, waiting until they are healthy.
4. Starts the monitoring services. A monitoring failure never rolls the
   release back.
5. Probes `https://<domain>/api/health/ready` and `/`. If a service in step 3
   does not become healthy, or the probe fails, it goes back to the previous
   release by itself. On the very first deploy there is nothing to go back to,
   so it stops and leaves no `current` link.
6. Keeps the five newest release folders and prunes images older than 30
   days.

## Roll back

Going back is deploying an older version by name. From your laptop:

```bash
gh workflow run deploy.yaml -f version=v0.1.1
gh run list --limit 2
```

The workflow checks out that tag, so you get the compose files and scripts as
they were then. Its images are still on GHCR. The server keeps the five newest
release folders (`ls /srv/echo/releases`); an older one is uploaded again, so
that works too.

The deploy key may run only `upload <version>`, `deploy <version>` and
`rollback`. To go back one release without GitHub, for example when Actions is
down:

```bash
ssh -i ~/.ssh/echo_deploy deploy@<server-ip> rollback
```

The database is not rolled back. A migration must keep the previous release
working, so write them as additive changes first and clean up in a later
release.

## When a deploy fails

Open the failed run in the Actions tab, or:

```bash
gh run view <run-id> --log-failed | tail -40
```

The log ends with the cause. [Troubleshooting](../troubleshooting.md) lists
the errors we have already hit, by their exact text.
