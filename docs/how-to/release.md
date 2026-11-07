# Release, watch and roll back

A release is a version tag on `main`, like `v0.1.2`. The tag builds the
images once and deploys exactly those images to the server. Nothing ships on
an ordinary merge.

Releasing needs the server from [Set up a server](./set-up-a-server.md).

## Cut a release

Make sure CI is green on the commit you want. Nothing enforces it.

```bash
gh release create v0.1.2 --target main --title v0.1.2 --generate-notes
```

`--target main` puts the tag on the latest commit of `main`, not on whatever
you have checked out. Version names are `vMAJOR.MINOR.PATCH`; anything else
is rejected by the workflow and by the server.

Tags are permanent. Never move or re-push one: its images would be
overwritten under the same name. If a release is bad, cut the next version.

## Watch it

```bash
gh run list --limit 4
```

Two runs appear, in order:

1. **Release** checks the tag is on `main`, then builds the 12 images in
   parallel and pushes them to GHCR as `:v0.1.2`. The first build with no
   cache takes 5 to 15 minutes.
2. **Deploy** starts when Release succeeds. It uploads the release folder to
   the server and runs `deploy.sh` there. It waits for approval if the
   `production` environment has required reviewers.

When it works, the end of the Deploy log says `Done`.

## What deploy.sh does

1. Pulls the images. If the pull fails, nothing has changed.
2. Starts the database and runs `migrator --check`. If migrations are
   pending (exit code 10) it takes a dump first, then migrates.
3. Points `current` at the new release and starts `db`, `api`, `nginx`,
   `backup` and `certbot`, waiting until they are healthy.
4. Starts the monitoring services. A monitoring failure never rolls the
   release back.
5. Probes `https://<domain>/api/health/ready` and `/`. If any step from 3
   fails, it goes back to the previous release by itself.
6. Keeps the five newest release folders and prunes images older than 30
   days.

## Roll back

Going back is deploying an older version by name. From your laptop:

```bash
gh workflow run deploy.yaml -f version=v0.1.1
gh run list --limit 2
```

The workflow checks out that tag, so you get the compose files and scripts as
they were then. Its images are still on GHCR, and the release folder is
usually still on the server.

To go back one release without GitHub, for example when Actions is down:

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
