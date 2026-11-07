# Echo docs

Pick by what you want to do.

## Start here

- [Getting started](./getting-started.md): run Echo on your machine.

## How to

Steps for one job. Each assumes you know what it is for.

- [Set up a server](./how-to/set-up-a-server.md): from a fresh Hetzner (Ubuntu or Debian) server to a running release.
- [Release, watch and roll back](./how-to/release.md)
- [Back up and restore the database](./how-to/restore-a-backup.md): where the nightly dumps are, and how to restore one.
- [Migrations](./how-to/migrations.md)
- [Rotate the JWT keys](./how-to/rotate-jwt-keys.md)
- [Run a load test](./how-to/run-a-load-test.md)
- [Troubleshooting](./troubleshooting.md): errors we have hit, by their text.

## Reference

Look something up.

- [Environment variables](./reference/environment.md)
- [Services, ports and volumes](./reference/services.md)
- [nginx](./reference/nginx.md)
- [CI and workflows](./reference/ci-and-workflows.md)
- [The API](./api/api-usage.md) and [getting started with it](./api/getting-started.md)

## Understand

Why it is the way it is.

- [How Echo is put together](./explanation/topology.md): the services, and why one server, built once and deployed as the same images.
- [Observability](./explanation/observability.md)
- [The status page](./explanation/status-page.md)
- [Limits](./explanation/limits.md): how much load it carries, and what happens if the server dies.
- [Backend architecture](./backend/architecture.md) and [testing](./backend/testing.md)
- [Frontend](./frontend/README.md): [architecture](./frontend/architecture.md), [components](./frontend/components.md)
- [Performance plan](./performance/plan.md) and [log](./performance/log.md): measured on the earlier 2 vCPU setup.

## Writing docs here

- A page does one job: a how-to, a reference, an explanation or a tutorial. If
  a how-to needs a paragraph of why, link to an explanation page.
- Start with what the thing is and what it is not. Put commands in bare code
  blocks with the expected output after them, and say where to run them.
- Limits are numbers with a date and how they were measured.
- Troubleshooting headings are the exact error text. Add an entry only after a
  real incident.
- No bylines or "last updated" lines: git has the history.
- Change the docs in the same pull request as the thing they describe. If a
  page can no longer be trusted, fix it or delete it; do not leave it
  misleading.
- Diagrams are text (Mermaid) so they diff and review like code.
- File names are kebab-case.
- Links between pages are relative. CI fails on a broken link and on a page
  nothing links to, so add a new page to this index.
