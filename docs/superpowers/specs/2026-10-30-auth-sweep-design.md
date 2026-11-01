# Auth sweep: design

Status: draft for review. Nothing here is implemented.
Scope: `backend/src/Echo.Auth` plus the shared email/hashing services it uses. Backend only. The frontend screens (getting-started card, verify-account reminder) are being designed separately and are out of scope.

## Why

Most of the slow routes in the API are auth routes. Reading the code, three causes account for it:

1. **bcrypt.** Login, registration, password reset and profile password-change each run a bcrypt hash or verify (250-400 ms observed at cost 10).
2. **Email is sent inline.** Verification, password reset and invitations `await` Resend before responding, so response time depends on a third party.
3. **Redundant queries and awkward flows.** Refresh, logout, revoke-all and registration each do queries they don't need, and a few flows are wrong, not just slow (see Phase 2).

Separately, the services are hard to read: roughly half of each method is span, metric and log calls around about ten lines of logic.

## Goals

- Auth endpoints no longer wait on Resend.
- Fewer database round-trips on login, refresh, logout, registration and password reset.
- Fix the flows that are wrong (invitation use burned on failed signup, unbounded reset tokens, revoke-all loading rows into memory).
- Auth service methods read as business logic, without losing the observability we have.

## Non-goals

- No change of password algorithm. **bcrypt stays at cost factor 10** (decided). No Argon2 or PBKDF2 migration; the database holds only seeded dev data.
- No durable email outbox. Emails queued in memory are lost if the API restarts. Accepted: the user can re-request verification or reset, and invitation codes are returned to the admin in the response.
- No roles/policies work (deferred until after deployment).
- No new email provider, domain or sender address. The `onboarding@resend.dev` sender stays until a domain is bought.
- No mediator/MessageBus library, no AOP or source-generated tracing.

## Anti-goals (would make this a failure even if it works)

- A signup, reset or invite that silently fails with no log line when the email can't be sent.
- Observability regressions: existing metrics (`auth.result` tag and counters) and the service-level spans must keep working.
- A clever abstraction layer for tracing that is harder to read than the code it replaces.

## Decided

| Topic | Decision |
|---|---|
| Hashing | bcrypt, cost 10. |
| Email delivery | In-memory queue plus background worker. Shared component in `Echo.Shared` next to `IEmailService`. |
| Retries | Up to 3 retries after the first attempt (4 attempts in total), back-off 2 s, 4 s, 8 s. Retry only on network errors, HTTP 429 and 5xx. Fail immediately on other 4xx. After the final failure, log an error with email type and recipient user/congregation ids. |
| Publishing | Auth publishes a message carrying the recipient and an `IEmailContent`. It does not build transport requests or call Resend. Any future feature can publish the same message. |
| Verification token in response | Stays in the success message until a domain exists (deliberate, for testing against Resend's sandbox sender). Not part of this work. Remove when a domain is bought. |
| Phase 3 | Required, but only in a form that stays clean. If it can't, stop and leave the code as it is. |

## Phase 1: Decouple email from the request

1. Add to `Echo.Shared`: the message type (recipient plus `IEmailContent`), a bounded in-memory queue, and a `BackgroundService` worker that drains it using the existing `ResendEmailService` and applies the retry policy above. Publishing never blocks the request; if the queue is full, log and drop.
2. Switch the three callers from `await emailService.SendAsync(...)` to publishing a message:
   - `EmailVerificationService.SendVerificationLinkToEmail`
   - `PasswordResetService.SendForgotPasswordLinkToEmail`
   - `InvitationService.CreateAndEmail`
3. `RegistrationService.RegisterUser` stops calling `SendVerificationLinkToEmail`. It already holds the committed user, so it issues the token and publishes directly, without re-fetching the user by email or looking up an existing token. Extract what both paths share (issue token, build link, publish) into one method that takes the user.
4. Remove the duplicate `IEmailService` registration (keyed `"Resend"` in `RegisterServices.cs` vs unkeyed in `Auth/Extensions.cs`). Keep one and update the three injection sites.
5. Remove the `Task.Run` wrapper in `BcryptHashService`; call BCrypt directly. (My call: it only moves the work to another threadpool thread.)
6. Close the login timing leak: when the email is unknown, run a bcrypt verify against a fixed dummy hash so response time doesn't reveal which emails have accounts. (My proposal; not yet confirmed.)

7. **Signups log you in** (added after review). `RegisterUser` and `RegisterCongregation` return a token pair (access + refresh), the same shape login returns, so nobody signs in again right after signing up. The session is issued in the same commit as the user. Email verification is not required to get a session (the frontend shows a "verify your account" reminder instead). This changes both endpoints' response from a plain message to a token pair; the endpoint descriptions that say "the admin is not logged in" must be rewritten, and the frontend needs to know.

**Consequence for invitations.** `CreateAndEmail` currently returns `EmailedTo` only if the send succeeded. With a queue it can't know. Decision (my call): `EmailedTo` means "queued for delivery". The response still contains the invitation code, so an admin can copy it if the email never arrives. Document this in the endpoint description and tell the frontend.

**Open for you:** `RegisterCongregation` never sends a verification email, but its endpoint docs say it does. Default in this design: send it, through the same shared path (the admin needs to verify like anyone else). Say if you'd rather fix the docs instead.

## Phase 2: Queries and flows

| # | Problem | Change |
|---|---|---|
| 1 | Refresh loads the token with its user, then `Revoke` re-selects the token by id, then `SessionService` fetches the user a third time. | Revoke on the loaded entity. Return the already-loaded user from `ValidateAndRotate`; drop the extra `GetByIdAnyCongregation`. |
| 2 | `RevokeAllActiveSessionsForUser` loads all active tokens and sets `RevokedAt` in a loop. | Single `ExecuteUpdate`. **Constraint:** `ExecuteUpdate` runs immediately, outside `IUnitOfWork.CommitAsync`. In password reset and profile password-change it must not run before the password change is committed, or it must share a transaction with it. |
| 3 | `RevokeToken` joins the user it doesn't use, then re-queries by id. | Query the token without the user and update it directly. |
| 4 | Registration burns an invitation use if the user insert later fails (`TryClaimUse` is a separate auto-committed statement). Two requests with the same email can also hit the unique index and surface as a 500. | Claim the use and insert the user atomically (one transaction). Catch the unique-index violation and return the same 400 "Email already in use". Needs a test that reproduces the burned use first. |
| 5 | Forgot-password creates a new reset token on every request and never invalidates earlier ones, so several reset links are valid at once and each request sends mail. | Mirror verification: invalidate the previous active token, rate-limit to one per 60 s, return the same response either way. Likely needs an `InvalidatedAt` column on the reset token (migration). |
| 6 | Email-verification and reset tokens are never pruned (refresh tokens are). | Opportunistic prune when a new token is issued, same pattern as `PruneTokens`. |
| 7 | `TokenHash` indexes on both token tables are non-unique. | Make unique (migration). |

Migrations are applied by hand; list each one in the PR description.

## Phase 3: Readable services without losing observability

Problem: every outcome in a service method repeats a span tag, a metric increment and a log call, and many repository calls are wrapped in their own `StartActivity` block.

Proposed shape (my guess; builder confirms it reads cleanly before committing to it):

- **One call per outcome.** Replace each tag + metric + log triple with a single per-service helper, for example `Fail("token_expired", ...)`, that sets `auth.result` on the span, increments the counter and writes the log. Result codes and metric tags stay identical.
- **Keep the service-level span; drop the inner spans** that wrap a single repository call, if database query spans already appear under it (check what `AuthInstrumentation.ConfigureTracing` and EF instrumentation export; if they don't, keep the inner spans).
- **Move pure rules out of the services** where they obscure the flow, for example token validity checks (expired, used, invalidated) shared by verification and reset.
- No decorators, no reflection, no generated code.

Exit test: open `RegistrationService.RegisterUser` and `SessionService.Login` after the change. If they don't read noticeably shorter and clearer, revert phase 3 and note why here.

## Testing

- Tests with every fix, per project convention. For phase 2 item 4, write the failing test first.
- Email worker: success, retry then success, permanent 4xx (no retry), exhausted retries (logged, no throw), queue full.
- Registration/reset/invitation: the HTTP response does not depend on the email service (use a fake that hangs or throws).
- Existing auth tests must pass unchanged unless a documented behaviour changed (invitation `EmailedTo`).
- Run scoped tests per phase, and `dotnet test backend/Echo.slnx` once before push.

## Docs to update with the code

`docs/backend/architecture.md` (email queue, new flow), endpoint descriptions (`RegisterCongregation`, invitation `EmailedTo`), `docs/api/` if it mentions registration or verification behaviour.

## Order

Phase 1, then Phase 2, then Phase 3 (so Phase 3 refactors the already-simplified code). One PR per phase, each starting as a draft per the repo workflow.
