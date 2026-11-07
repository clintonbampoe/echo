# Echo

Quiet software for loud communities. Echo is church management for
congregations: one profile for every person, Sunday attendance taken on a
phone, tithes and expenses on one chart, and a dashboard that answers "how many
came on Sunday?" without a spreadsheet.

Every congregation is its own private space. A sign-in only ever sees its own
church's records, and amounts are in Ghana cedis (GHS) with Ghana's regions
built in.

![The Echo dashboard: member, attendance, tithe and income figures, attendance and visitor cards](./docs/diagrams/DashboardScreenshot.png)

## What it does

| Area | What you can do |
|---|---|
| Members | One profile per person: contacts, address, region, emergency contacts, and their attendance, giving and groups |
| Visitors | Flag first-time guests, follow them up, and turn them into members in one step |
| Attendance | Take the register on a phone during the service; members and visitors counted apart; session types of your own |
| Events | Plan, take registrations, and check people in on the day |
| Tithes | Record each tithe against the right member, with amount, method and the month it is for |
| Finance | Income and expenses side by side, with your own categories |
| Projects | Set a target, record each contribution, and watch the progress |
| Ministries and groups | Choirs, youth, ushers and cell groups, each with its own members |
| Assets | What the church owns and what it is worth |
| Team | Invite leaders with a code; four roles (Admin, Accountant, Clerk, Member) decide what each can do |

Charts and figures on the dashboard open the records behind them.

## How it is built

An ASP.NET Core API (minimal APIs, Entity Framework Core) on PostgreSQL 18, a
React 19 and TypeScript web app, and nginx in front. It runs as one Docker
Compose project on a single server and ships as versioned container images.
Metrics, logs and traces go to Grafana, and a Gatus page shows whether each
service and nightly job is up. Tests are xUnit v3 with Testcontainers for the
backend and Vitest for the frontend.

Echo is built for a small self-hosted deployment. It has no failover and one
server's capacity; [Limits](./docs/explanation/limits.md) says what that means.

## Run it on your machine

You need Docker with the `docker compose` plugin, Git, the .NET 10 SDK (the
key script runs it) and a bash shell (on Windows, Git Bash or WSL).

```bash
git clone https://github.com/clintonbampoe/echo.git && cd echo
cp .env.example .env
# edit .env: MailClient__Address and Resend__ApiKey (ask a maintainer)
bash backend/tools/jwt-key-setup/SetupJwtKeys.sh env
bash backend/tools/invitation-key-setup/SetupInvitationKey.sh env
docker compose up
```

The two scripts write generated keys into `.env`. Without real Resend values,
email (verification, invitations, password reset) does not send. Open
http://localhost:8080 and create a church at `/signup/church`; the API
reference is at http://localhost:8080/scalar. The full walk-through, with the
output of each step, is [Getting started](./docs/getting-started.md).

## Run it for real

A release is a version tag on `main`: `gh release create v0.1.2 --target main
--generate-notes` builds the images and deploys them to the server, and going
back is deploying an older version. To set up a server and ship the first
release, follow [Set up a server](./docs/how-to/set-up-a-server.md) and
[Release](./docs/how-to/release.md). The rest of [`docs/`](./docs/README.md) is
organised by what you want to do: how-tos, reference, and explanations of why
it is built this way.

## Contributing

- Bugs and feature requests: open an issue from a template.
- Security problems: do not open a public issue; see [SECURITY.md](./SECURITY.md).
- Code and review process: [CONTRIBUTING.md](./CONTRIBUTING.md).

Before pushing: `dotnet test --solution backend/Echo.slnx --filter "Category=Unit"`
and `cd frontend && npm run check`.

## License

See [LICENSE.md](./LICENSE.md).
