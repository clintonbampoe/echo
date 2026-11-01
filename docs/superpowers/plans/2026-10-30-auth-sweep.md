# Auth Sweep Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Take email sending off the auth request path, remove redundant queries and fix wrong flows in `Echo.Auth`, and make the auth services readable without losing observability.

**Architecture:** A shared in-memory email queue plus a `BackgroundService` worker (retry with back-off) lives in `Echo.Shared`; auth services publish `EmailMessage`s instead of calling Resend. Phase 2 fixes repository/service queries and flows, tested against a real Postgres through a new Testcontainers fixture. Phase 3 collapses repeated span-tag + metric code into one helper.

**Tech Stack:** ASP.NET Core 10, EF Core + Npgsql, `System.Threading.Channels`, xUnit v3, Testcontainers (PostgreSQL, new test-only dependency), Resend SDK 0.8.0.

**Spec:** `docs/superpowers/specs/2026-10-30-auth-sweep-design.md`

## Global Constraints

- bcrypt stays at **cost factor 10**. No algorithm change.
- Retries: **3 retries after the first attempt (4 attempts)**, back-off **2 s, 4 s, 8 s**; retry only network errors, 429 and 5xx; other 4xx fail immediately; final failure logs an error with email kind and user/congregation ids.
- Queue is in memory and bounded; publishing never blocks a request; if full, log and drop.
- No outbox, no mediator library, no AOP/source-generated tracing, no new email provider or sender address (`onboarding@resend.dev` stays).
- The verification token stays in the `SendVerificationLinkToEmail` success message (deliberate until a domain exists).
- Existing metric names and the `auth.result` tag values do not change. Service-level spans stay.
- Backend only. Migrations are created by `dotnet ef migrations add` but **applied by hand**; list each in the PR description.
- One PR per phase, each opened as a Draft. Tests with every fix; run scoped tests per task, the full `dotnet test backend/Echo.slnx` once before push.
- Test conventions: xUnit v3, `[Trait("Category", "Unit")]` for DB-free tests (the only category CI runs) and `[Trait("Category", "Integration")]` for Postgres-backed ones. Hand-written fakes; there is no mocking library.

## Review Focus

1. Queue full: publish returns false, the request still succeeds, a warning is logged (Task 2).
2. Resend rejects an address with a permanent 4xx: exactly one attempt, no retries (Task 2).
3. API shuts down while the worker is waiting to retry: no exception escapes, the host stops promptly (Task 2).
4. Two registrations with the same email at once: the loser gets 400 "Email already in use", not a 500, and the invitation use is not consumed (Task 8).
5. Forgot-password for an unknown email, and repeated requests within 60 s: same response as a known email, no email queued for the unknown one, only one email for the repeats (Task 9).
6. A reused (already rotated) refresh token still revokes every session after the revoke-all rewrite (Task 7).

---

## Phase 1: Email off the request path (PR 1)

### Task 1: Postgres-backed test fixture

**Files:**
- Modify: `backend/Directory.Packages.props` (add `Testcontainers.PostgreSql`, latest stable at least two weeks old)
- Modify: `backend/tests/Echo.IntegrationTests/Echo.IntegrationTests.csproj` (package reference)
- Create: `backend/tests/Echo.IntegrationTests/Support/PostgresFixture.cs`
- Create: `backend/tests/Echo.IntegrationTests/Support/CommandCounter.cs`
- Create: `backend/tests/Echo.IntegrationTests/Support/AuthTestHost.cs`
- Delete: `backend/tests/Echo.IntegrationTests/UnitTest1.cs`
- Test: `backend/tests/Echo.IntegrationTests/Support/FixtureSmokeTests.cs`

**Interfaces:**
- Produces:
  - `PostgresFixture : IAsyncLifetime` with `string ConnectionString`; starts one `postgres:18` container per test class collection and applies all migrations (`AppDbContext.Database.MigrateAsync`).
  - `CommandCounter : DbCommandInterceptor` with `int Count` and `void Reset()`; counts every executed DB command.
  - `AuthTestHost` with `static AuthTestHost Create(PostgresFixture db, Action<IServiceCollection>? configure = null)`, `T Get<T>()` (resolves from a fresh scope, shared across calls on the same host), `CommandCounter Commands`. It builds a `ServiceCollection` with `AddSharedServices`, `AddAuthServices`, the Data layer registrations used by `Echo.Api` (copy the registration calls from `Program.cs`), the `CommandCounter` on `AppDbContext`, and a fake `FakeTimeProvider`. Each test starts from an empty schema (truncate tables between tests).

- [ ] **Step 1: Write the smoke test** `FixtureSmokeTests.CanCreateUserAndReadItBack`: build an `AuthTestHost`, insert a `User` via `UserRepository` + `IUnitOfWork.CommitAsync`, read it back with `GetByEmail`, assert the email matches and `Commands.Count > 0`.
- [ ] **Step 2: Run it, expect FAIL** (types missing): `dotnet test --project backend/tests/Echo.IntegrationTests --filter "FullyQualifiedName~FixtureSmokeTests"`.
- [ ] **Step 3: Implement the three support types** as specified in Interfaces. Reuse the DbContext/Npgsql registration from `Echo.Api`'s `Program.cs`/`Echo.Data/Extensions.cs` instead of re-deriving it. If `dotnet test` rejects `--filter`, use `--filter-class`.
- [ ] **Step 4: Run it, expect PASS.** Requires Docker.
- [ ] **Step 5: Commit** `test: add postgres-backed fixture for auth tests`.

### Task 2: Email queue and delivery worker

**Files:**
- Create: `backend/src/Echo.Shared/Services/Email/EmailMessage.cs`
- Create: `backend/src/Echo.Shared/Services/Email/IEmailQueue.cs`
- Create: `backend/src/Echo.Shared/Services/Email/EmailQueue.cs`
- Create: `backend/src/Echo.Shared/Services/Email/EmailRetryPolicy.cs`
- Create: `backend/src/Echo.Shared/Services/Email/EmailDeliveryWorker.cs`
- Modify: `backend/src/Echo.Shared/Extensions/RegisterServices.cs`
- Test: `backend/tests/Echo.Shared.Tests/Services/Email/EmailQueueTests.cs`, `EmailRetryPolicyTests.cs`, `EmailDeliveryWorkerTests.cs`

**Interfaces:**
- Produces:
  - `public sealed record EmailMessage(string To, IEmailContent Content, string Kind, Guid? UserId = null, Guid? CongregationId = null);` (`Kind` e.g. `"email_verification"`, `"password_reset"`, `"invitation"`; used in logs only.)
  - `public interface IEmailQueue { bool TryEnqueue(EmailMessage message); }`
  - `public sealed class EmailQueue : IEmailQueue` with `public const int Capacity = 1000;` and `public ChannelReader<EmailMessage> Reader { get; }`. Bounded `Channel`, `FullMode = DropWrite`; `TryEnqueue` returns the `TryWrite` result and logs a warning when it returns false.
  - `public static class EmailRetryPolicy { public static readonly TimeSpan[] Delays = [2s, 4s, 8s]; public static bool IsRetryable(Exception ex); }`. Retryable: `ResendException` with `IsTransient == true` (verify in Resend 0.8.0 that this covers 429 and 5xx; if not, check `StatusCode` directly), `HttpRequestException`, and `TaskCanceledException`/`TimeoutException` that is not caused by the shutdown token. Everything else is not.
  - `public sealed class EmailDeliveryWorker : BackgroundService`, ctor `(EmailQueue queue, IServiceScopeFactory scopes, TimeProvider time, ILogger<EmailDeliveryWorker> logger)`. Loop: read from `queue.Reader`, resolve `IEmailService` in a new scope, `SendAsync`; on a retryable failure wait the next delay with `Task.Delay(delay, time, stoppingToken)` and retry; after the last failure or on a non-retryable one, `logger` error with kind, userId, congregationId, and move on. A failure never stops the loop. `OperationCanceledException` from `stoppingToken` ends the loop quietly.
  - `AddSharedServices` registers `EmailQueue` as singleton (also as `IEmailQueue`), `AddHostedService<EmailDeliveryWorker>`, and a single unkeyed scoped `IEmailService` -> `ResendEmailService`; the keyed `"Resend"` registration is removed.
- Consumes: existing `IEmailService.SendAsync(string to, IEmailContent content)`.

- [ ] **Step 1: Write failing tests** (names and assertions):
  - `EmailQueueTests.TryEnqueue_WhenFull_ReturnsFalseAndKeepsTheFirstMessages` (fill to `Capacity`, next call false, `Reader.Count == Capacity`).
  - `EmailRetryPolicyTests.Delays_AreTwoFourEightSeconds`; `IsRetryable_NetworkError_True`; `IsRetryable_ServerError_True`; `IsRetryable_RateLimited_True`; `IsRetryable_BadAddress4xx_False`.
  - `EmailDeliveryWorkerTests` using a fake `IEmailService` (scriptable: throws N times then succeeds) and `FakeTimeProvider`: `Send_Succeeds_FirstAttempt_SendsOnce`; `Send_TransientFailureThenSuccess_RetriesWithBackOff` (advance time 2 s, assert second attempt, 4 s, third); `Send_PermanentFailure_TriesOnce`; `Send_AlwaysTransient_StopsAfterFourAttemptsAndLogsError`; `Worker_KeepsProcessing_AfterAMessageFailsFinally`; `Stop_WhileWaitingToRetry_EndsWithoutThrowing`.
- [ ] **Step 2: Run, expect FAIL**: `dotnet test --project backend/tests/Echo.Shared.Tests --filter "FullyQualifiedName~Email"`.
- [ ] **Step 3: Implement** the types above. Add `Microsoft.Extensions.Logging.Abstractions`/`Hosting.Abstractions` references to `Echo.Shared.csproj` only if missing.
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit** `feat(shared): in-memory email queue with retrying delivery worker`.

### Task 3: Verification and password reset publish instead of send

**Files:**
- Modify: `backend/src/Echo.Auth/EmailVerifications/EmailVerificationService.cs`
- Modify: `backend/src/Echo.Auth/Passwords/PasswordResetService.cs`
- Modify: `backend/src/Echo.Auth/Extensions.cs` (delete the unkeyed `IEmailService` registration; keep `AddResend`)
- Test: `backend/tests/Echo.IntegrationTests/Auth/EmailPublishingTests.cs`

**Interfaces:**
- Consumes: `IEmailQueue.TryEnqueue`, `EmailMessage` (Task 2); `AuthTestHost` (Task 1).
- Produces on `EmailVerificationService`:
  - `public async Task<IOperationResult> SendVerificationLink(User user, CancellationToken ct)`: contains everything after the user lookup in today's `SendVerificationLinkToEmail` (rate-limit check, invalidate old token, create token, commit, build link, `TryEnqueue(new EmailMessage(user.EmailAddress, content, "email_verification", user.Id, user.CongregationId))`). Returns the same success messages as today, including the `Token: {token}` text.
  - `SendVerificationLinkToEmail(string emailAddress, CancellationToken ct)` becomes lookup-by-email + `SendVerificationLink`; behaviour and messages unchanged.
- `PasswordResetService.SendForgotPasswordLinkToEmail` enqueues `new EmailMessage(user.EmailAddress, content, "password_reset", user.Id, user.CongregationId)` instead of awaiting `emailService`.
- Neither service injects `IEmailService` any more; both inject `IEmailQueue`.

- [ ] **Step 1: Write failing tests** with a `RecordingEmailQueue : IEmailQueue` fake registered through `AuthTestHost.Create(..., configure)`, and an `IEmailService` that blocks forever (proves nothing awaits it): `SendVerificationLink_QueuesOneEmailToTheUser_AndReturnsWithoutSending`; `SendVerificationLinkToEmail_UnknownEmail_QueuesNothing_SameMessageAsBefore`; `SendVerificationLink_WithinSixtySeconds_QueuesNothingTheSecondTime`; `SendForgotPasswordLink_QueuesOneEmail_AndReturnsWithoutSending`; `SendForgotPasswordLink_UnknownEmail_QueuesNothing_SameResultMessage`.
- [ ] **Step 2: Run, expect FAIL**: `dotnet test --project backend/tests/Echo.IntegrationTests --filter "FullyQualifiedName~EmailPublishingTests"`.
- [ ] **Step 3: Implement** per Interfaces. Keep span names, metric calls and log calls as they are (Phase 3 handles them). Remove the `email.send` span and the `email.provider` tags from these paths; the worker owns sending now.
- [ ] **Step 4: Run, expect PASS.** Also `dotnet build backend/Echo.slnx` to catch other `IEmailService` injection sites.
- [ ] **Step 5: Commit** `refactor(auth): publish verification and reset emails to the queue`.

### Task 4: Invitations publish; `EmailedTo` means queued

**Files:**
- Modify: `backend/src/Echo.Auth/Invitations/InvitationService.cs` (`CreateAndEmail`)
- Modify: `backend/src/Echo.Auth/Invitations/InvitationEndpoints.cs` (description text for the by-email endpoint and `InviteResponseDto.EmailedTo`)
- Test: `backend/tests/Echo.IntegrationTests/Auth/InvitationEmailQueueTests.cs`

**Interfaces:**
- Consumes: `IEmailQueue`, `EmailMessage`.
- `CreateAndEmail` keeps its signature. It enqueues `new EmailMessage(emailAddress, content, "invitation", createdByUserId, congregationId)`. `EmailedTo` is set to `emailAddress` when `TryEnqueue` returns true, otherwise null. The response still includes the invitation code. The try/catch around the send is removed (nothing throws any more).
- `InvitationService` no longer injects `IEmailService`.

- [ ] **Step 1: Write failing tests**: `CreateAndEmail_QueuesTheInvitationEmail_AndReportsEmailedTo`; `CreateAndEmail_WhenQueueRefuses_StillCreatesTheInvitation_WithNullEmailedTo`; `CreateAndEmail_ReturnsWithoutWaitingForTheEmailService` (blocking `IEmailService`).
- [ ] **Step 2: Run, expect FAIL** (`--filter "FullyQualifiedName~InvitationEmailQueueTests"`).
- [ ] **Step 3: Implement** and update the endpoint description: `emailedTo` means "queued for delivery"; the code is returned so it can be copied.
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit** `refactor(auth): queue invitation emails`.

### Task 5: Registration without inline email, congregation admin gets verification

**Files:**
- Modify: `backend/src/Echo.Auth/Registrations/RegistrationService.cs`
- Modify: `backend/src/Echo.Auth/Registrations/RegistrationEndpoints.cs` (confirm the `RegisterCongregation` description is now true; fix `RegisterUser` description if it mentions email timing)
- Test: `backend/tests/Echo.IntegrationTests/Auth/RegistrationEmailTests.cs`

**Interfaces:**
- Consumes: `EmailVerificationService.SendVerificationLink(User user, CancellationToken ct)` (Task 3).
- `RegisterUser`: replace `emailVerificationService.SendVerificationLinkToEmail(user.EmailAddress, ct)` with `SendVerificationLink(user, ct)` (no re-fetch by email). Remove the `svc.registration.persist.email_verification` span's misleading name by renaming it `svc.registration.queue.email_verification`.
- `RegisterCongregation`: after `CommitAsync`, call `SendVerificationLink(user, ct)` for the admin user.

- [ ] **Step 1: Write failing tests**: `RegisterUser_QueuesOneVerificationEmail_WithoutExtraUserLookup` (assert `Commands` count for the email part is lower than before: record the count with a comment of the expected number once measured); `RegisterUser_ReturnsWithoutWaitingForTheEmailService`; `RegisterCongregation_QueuesAVerificationEmailForTheAdmin`.
- [ ] **Step 2: Run, expect FAIL** (`--filter "FullyQualifiedName~RegistrationEmailTests"`).
- [ ] **Step 3: Implement** per Interfaces.
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Docs:** update `docs/backend/architecture.md` (email queue and worker, who publishes) and any `docs/api/` page that describes registration or invitation email behaviour.
- [ ] **Step 6: Commit** `feat(auth): registration no longer waits on email` and open the Phase 1 PR as Draft.

### Task 5a: Signups return a session

**Files:**
- Modify: `backend/src/Echo.Auth/Sessions/SessionService.cs`, `backend/src/Echo.Auth/Sessions/JwtTokenService.cs`
- Modify: `backend/src/Echo.Auth/Registrations/RegistrationService.cs`, `RegistrationEndpoints.cs`
- Test: `backend/tests/Echo.IntegrationTests/Auth/SignupSessionTests.cs`

**Interfaces:**
- Produces on `SessionService`: `public async Task<TokenPairResponseDtos> StartSession(User user, CancellationToken ct)`: maps the user with `IUserMapper.ToAuthDto`, generates the access token, calls `JwtTokenService.IssueToken`, and returns the pair. It does **not** commit; the caller's single `CommitAsync` saves user and refresh token together. `Login` is changed to call it, so there is one place that builds a session.
- `JwtTokenService.IssueToken(Guid userId, CancellationToken ct = default, bool prune = true)`: `StartSession` for a brand-new user passes `prune: false` (nothing to prune), so signup skips the extra DELETE. Login keeps `prune: true`.
- `RegistrationService` injects `SessionService`. `RegisterUser` and `RegisterCongregation` call `StartSession(user, ct)` before their commit and return `SuccessResult<TokenPairResponseDtos>` instead of `SuccessResult<string>`. Failure paths (invalid invitation, policy, email taken) are unchanged and issue no session.
- Both endpoint descriptions are rewritten: success returns the token pair; remove "the admin is not logged in" and "do not assume the response carries tokens"; say the account is unverified and the client should show the verify reminder.

- [ ] **Step 1: Write failing tests**: `RegisterUser_ReturnsATokenPair_AndTheRefreshTokenWorks` (feed the returned refresh token to `SessionService.RefreshAccessToken`, expect success); `RegisterCongregation_ReturnsATokenPair_ForTheNewAdmin` (decode the access token: role `Admin`, the new congregation id); `RegisterUser_EmailTaken_ReturnsNoSession_AndStoresNoRefreshToken`; `Signup_SavesUserAndSessionInOneCommit` (assert the commit count via `Commands`, no extra DELETE).
- [ ] **Step 2: Run, expect FAIL**: `dotnet test --project backend/tests/Echo.IntegrationTests --filter "FullyQualifiedName~SignupSessionTests"`.
- [ ] **Step 3: Implement** per Interfaces.
- [ ] **Step 4: Run, expect PASS**, plus the Task 5 registration tests (their expectations of the result type change from `string` to `TokenPairResponseDtos`; update them).
- [ ] **Step 5: Docs:** update `docs/api/` registration description. In the PR description, flag the response change for the frontend.
- [ ] **Step 6: Commit** `feat(auth): signups return a session`.

### Task 6: bcrypt without `Task.Run`, constant-work login

**Files:**
- Modify: `backend/src/Echo.Shared/Services/Hashing/BcryptHashService.cs`
- Modify: `backend/src/Echo.Auth/Sessions/SessionService.cs` (`Login`)
- Test: `backend/tests/Echo.Shared.Tests/Services/Hashing/BcryptHashServiceTests.cs`, `backend/tests/Echo.IntegrationTests/Auth/LoginTimingTests.cs`

**Interfaces:**
- `IPasswordHasher` keeps `Task<string> HashAsync(string)` and `Task<bool> VerifyAsync(string, string)`. `BcryptHashService` computes synchronously and returns `Task.FromResult(...)`. Cost stays `10`.
- `SessionService` gets `private const string DummyHash = "<a valid bcrypt cost-10 hash of any string>"`. Generate it once with `BCrypt.Net.BCrypt.HashPassword("x", 10)` and paste the constant. In `Login`, when the user is not found, call `await hashService.VerifyAsync(password, DummyHash)` before returning the same `BadRequestResult("Email or password is invalid.")`.

- [ ] **Step 1: Write failing tests**: `BcryptHashServiceTests.HashThenVerify_RoundTrips`, `Hash_UsesCostFactorTen` (hash string starts with `$2a$10$` or `$2b$10$`), `Verify_WrongPassword_False`; `LoginTimingTests.Login_UnknownEmail_StillRunsOnePasswordVerification_AndReturnsTheSameError` (counting fake `IPasswordHasher`).
- [ ] **Step 2: Run, expect the login test to FAIL**.
- [ ] **Step 3: Implement** per Interfaces.
- [ ] **Step 4: Run, expect PASS** (`--filter "FullyQualifiedName~Bcrypt|FullyQualifiedName~LoginTiming"`).
- [ ] **Step 5: Commit** `fix(auth): constant-work login and no Task.Run around bcrypt`. Mark Phase 1 PR ready after the full `dotnet test backend/Echo.slnx` passes.

---

## Phase 2: Queries and flows (PR 2)

### Task 7: Refresh, logout and revoke-all queries

**Files:**
- Modify: `backend/src/Echo.Data/IUnitOfWork.cs`, `backend/src/Echo.Data/UnitOfWork.cs`
- Modify: `backend/src/Echo.Auth/Sessions/SessionRepository.cs`, `JwtTokenService.cs`, `SessionService.cs`, `RefreshTokenDtos.cs`
- Modify: `backend/src/Echo.Auth/Passwords/PasswordResetService.cs` (`ResetPassword`), `backend/src/Echo.Auth/Profile/ProfileService.cs` (`Update`)
- Test: `backend/tests/Echo.IntegrationTests/Auth/SessionQueryTests.cs`

**Interfaces:**
- `IUnitOfWork` gains `Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)`: begins a transaction on the shared `AppDbContext`, runs `work`, commits it, rolls back on exception.
- `SessionRepository.RevokeAllActiveSessionsForUser(Guid userId, CancellationToken ct)` becomes one `ExecuteUpdateAsync` setting `RevokedAt = now` where `UserId == userId && RevokedAt == null` (and not deleted). Return type `Task` (callers ignore the bool).
- `SessionRepository.Revoke(RefreshToken token, Guid? replacedByTokenId)` takes the loaded entity (sync, sets fields) instead of re-querying by id; `JwtTokenService.RevokeToken` loads the token **without** `Include(User)` (new `GetTokenRecordByHash`) and revokes it.
- `RefreshTokenValidationResult` gains `User? User` (the user already loaded by `GetTokenRecordByHashWithUser`); `SessionService.RefreshAccessToken` uses it and no longer calls `GetByIdAnyCongregation`.
- `ResetPassword` and `ProfileService.Update` (password branch) wrap "set new hash + `PasswordChangedAt` + mark token used + revoke all sessions + commit" in `ExecuteInTransactionAsync`, so the immediate `ExecuteUpdate` rolls back with the password change if anything fails.

- [ ] **Step 1: Write failing tests** (all against Postgres; `Commands` from the host):
  - `Refresh_ValidToken_RotatesWithAtMostFourCommands` (select token+user, prune delete, insert, commit; record the measured baseline in a comment and assert `<=` the new number).
  - `Refresh_ReusedToken_RevokesEverySession` (Review Focus 6).
  - `RevokeToken_RevokesWithoutLoadingTheUser`.
  - `RevokeAllActiveSessions_UsesOneStatement_AndRevokesOnlyActiveTokens`.
  - `ResetPassword_WhenCommitFails_DoesNotRevokeSessions` (force a failure, e.g. a fake `IPasswordHasher` that throws after the revoke is scheduled; assert sessions still active).
- [ ] **Step 2: Run, expect FAIL** (`--filter "FullyQualifiedName~SessionQueryTests"`).
- [ ] **Step 3: Implement** per Interfaces. `PruneTokens` already uses `ExecuteDelete` and stays as is.
- [ ] **Step 4: Run, expect PASS**; also run the existing `Echo.Auth.Tests` category Unit.
- [ ] **Step 5: Commit** `perf(auth): fewer queries on refresh and revoke`.

### Task 8: Registration is atomic and survives the email race

**Files:**
- Modify: `backend/src/Echo.Auth/Registrations/RegistrationService.cs` (`RegisterUser`)
- Modify: `backend/src/Echo.Auth/Invitations/InvitationService.cs` (`TryClaimUse` stays; ordering/transaction only)
- Test: `backend/tests/Echo.IntegrationTests/Auth/RegistrationAtomicityTests.cs`

**Interfaces:**
- Consumes: `IUnitOfWork.ExecuteInTransactionAsync` (Task 7).
- In `RegisterUser`, `TryClaimUse` and the user insert + commit run inside one `ExecuteInTransactionAsync`. A `DbUpdateException` whose inner `PostgresException.SqlState == "23505"` on the user email index returns `new BadRequestResult("Email already in use")` (and records `email_taken`), after the transaction has rolled back the claimed use. If `TryClaimUse` returns false, return the existing invalid-invitation result.

- [ ] **Step 1: Write failing tests first, reproducing the bugs on the current code:**
  - `RegisterUser_WhenUserInsertFails_DoesNotConsumeAnInvitationUse` (pre-insert a user with the same email *after* the `IsEmailTaken` check using a hook/second connection, or make the insert fail via the unique index; assert `UseCount` unchanged and a single-use invitation is still valid).
  - `RegisterUser_TwoConcurrentRequestsSameEmail_OneSucceeds_OtherGets400_NotAnException` (Review Focus 4).
  - `RegisterUser_LastUseOfLimitedInvitation_OnlyOneOfTwoConcurrentRequestsSucceeds`.
- [ ] **Step 2: Run, expect the first two to FAIL** (`--filter "FullyQualifiedName~RegistrationAtomicityTests"`).
- [ ] **Step 3: Implement** per Interfaces.
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit** `fix(auth): registration claims an invitation use atomically`.

### Task 9: Password reset tokens: one active, rate-limited, pruned

**Files:**
- Modify: `backend/src/Echo.Domain/Auth/PasswordResetToken.cs` (add `public DateTime? InvalidatedAt { get; set; }`)
- Modify: `backend/src/Echo.Data/Configurations/Auth/PasswordVerificationTokenConfiguration.cs`
- Modify: `backend/src/Echo.Auth/Passwords/PasswordResetRepository.cs`, `PasswordResetService.cs`
- Create: migration `AddPasswordResetTokenInvalidatedAt`
- Test: `backend/tests/Echo.IntegrationTests/Auth/PasswordResetFlowTests.cs`

**Interfaces:**
- `PasswordResetRepository.GetTokenByUserId` also requires `InvalidatedAt == null` and orders by `CreatedAt` descending (mirror `EmailVerificationRepository.GetTokenByUserId`).
- `SendForgotPasswordLinkToEmail`: if an active token was created in the last 60 s, return the same `SuccessResult<string>("Reset link has been sent.")` and queue nothing; otherwise set `InvalidatedAt = now` on the active token, create the new one, enqueue as in Task 3.
- `ResetPassword` rejects a token with `InvalidatedAt != null` as `InvalidTokenResult`.
- A new `PasswordResetRepository.PruneTokens(Guid userId, CancellationToken ct)` (`ExecuteDelete` of the user's tokens that are expired, used, or invalidated more than 7 days ago) is called when a new token is issued.

- [ ] **Step 1: Write failing tests**: `Forgot_Twice_InsideSixtySeconds_QueuesOneEmail_BothReturnTheSameMessage`; `Forgot_AfterSixtySeconds_InvalidatesTheOldLink` (old token then fails `ResetPassword`); `Forgot_UnknownEmail_SameMessage_NothingQueued`; `ResetPassword_InvalidatedToken_IsRejected`; `Forgot_PrunesOldTokensOfTheUser`.
- [ ] **Step 2: Run, expect FAIL** (`--filter "FullyQualifiedName~PasswordResetFlowTests"`).
- [ ] **Step 3: Implement** per Interfaces; generate the migration with the command in `AGENTS.md`.
- [ ] **Step 4: Run, expect PASS.**
- [ ] **Step 5: Commit** `fix(auth): one active password reset link at a time`.

### Task 10: Verification-token pruning and unique token hashes

**Files:**
- Modify: `backend/src/Echo.Auth/EmailVerifications/EmailVerificationRepository.cs`, `EmailVerificationService.cs`
- Modify: `backend/src/Echo.Data/Configurations/Auth/EmailVerificationTokenConfigurations.cs`, `PasswordVerificationTokenConfiguration.cs`
- Create: migration `UniqueAuthTokenHashes`
- Test: `backend/tests/Echo.IntegrationTests/Auth/TokenHygieneTests.cs`

**Interfaces:**
- `EmailVerificationRepository.PruneTokens(Guid userId, CancellationToken ct)`: same rule as Task 9 (expired, used, or invalidated more than 7 days ago); called when a new verification token is issued, before the insert.
- `HasIndex(e => e.TokenHash)` becomes `.IsUnique()` on both token configurations.

- [ ] **Step 1: Write failing tests**: `IssuingAVerificationToken_PrunesTheUsersDeadTokens_KeepsTheActiveOne`; `TwoTokensWithTheSameHash_AreRejectedByTheDatabase` (both tables).
- [ ] **Step 2: Run, expect FAIL** (`--filter "FullyQualifiedName~TokenHygieneTests"`).
- [ ] **Step 3: Implement**; generate the migration.
- [ ] **Step 4: Run, expect PASS.** Run `dotnet test backend/Echo.slnx` before pushing; mark the Phase 2 PR ready.
- [ ] **Step 5: Commit** `perf(auth): prune dead tokens, enforce unique hashes`.

---

## Phase 3: Readable services (PR 3)

### Task 11: One call for span result and metric

**Files:**
- Modify: `backend/src/Echo.Auth/AuthInstrumentation.cs`
- Modify: `backend/src/Echo.Auth/Sessions/SessionService.cs`, `Registrations/RegistrationService.cs` (first two, as the readability trial)
- Test: `backend/tests/Echo.Auth.Tests/AuthInstrumentationTests.cs`

**Interfaces:**
- Produces on `AuthInstrumentation`: `public void Record(Counter<long> counter, Activity? span, string result, string congregationId = "unknown", params (string Key, object? Value)[] extraTags)`. It sets `auth.result` on the span, then adds 1 to the counter with tags `auth.result` and `congregation.id` (plus extras). Log calls stay at the call site: they are source-generated and carry distinct messages.
- In the two trial services, replace each `span?.SetTag("auth.result", x); RecordY(x, id);` pair with one `instrumentation.Record(...)` call and delete the per-service `RecordX` private methods they made redundant.

- [ ] **Step 1: Write failing tests** with a `MeterListener`/`ActivityListener`: `Record_SetsTheResultTagOnTheSpan_AndIncrementsTheCounterWithTheSameTags`; `Record_DefaultsCongregationToUnknown`; `Record_WithNullSpan_StillIncrements`.
- [ ] **Step 2: Run, expect FAIL** (`dotnet test --project backend/tests/Echo.Auth.Tests --filter "FullyQualifiedName~AuthInstrumentationTests"`).
- [ ] **Step 3: Implement** `Record` and convert `SessionService.Login` and `RegistrationService.RegisterUser`.
- [ ] **Step 4: Run Task 11 tests and the Phase 1/2 integration tests touching these services, expect PASS** (metric names and tag values unchanged).
- [ ] **Step 5: Readability gate.** Compare `Login` and `RegisterUser` before and after. If they are not clearly shorter and clearer, `git revert` this task, add a line under "Phase 3" in the spec saying why, and stop Phase 3 here.
- [ ] **Step 6: Commit** `refactor(auth): record span result and metric in one call`.

### Task 12: Apply to the remaining services, drop redundant inner spans

**Files:**
- Modify: `EmailVerificationService.cs`, `PasswordResetService.cs`, `InvitationService.cs`, `ProfileService.cs`, `SessionService.cs` (refresh, logout, revoke paths)
- Modify: `backend/src/Echo.Auth/AuthInstrumentation.cs` (only if EF tracing is missing, see Step 1)

- [ ] **Step 1: Check tracing coverage.** Read `AuthInstrumentation.ConfigureTracing` and the API's OpenTelemetry setup. If EF Core/Npgsql spans are already exported, delete the `StartActivity(...)` blocks that wrap a single repository call (e.g. `svc.session.fetch.user_by_email`); if not, keep them and note it in the PR.
- [ ] **Step 2: Convert** the remaining services to `instrumentation.Record(...)`, keeping every `auth.result` value and counter exactly as is. Delete the `RecordX` helpers made redundant.
- [ ] **Step 3: Verify** `dotnet build backend/Echo.slnx` and the integration + unit tests for auth pass unchanged: `dotnet test --project backend/tests/Echo.IntegrationTests` and `dotnet test --project backend/tests/Echo.Auth.Tests`.
- [ ] **Step 4: Commit** `refactor(auth): use the shared result recorder in all services`.

### Task 13: Share token validity rules

**Files:**
- Create: `backend/src/Echo.Auth/TokenValidity.cs`
- Modify: `EmailVerificationService.cs`, `PasswordResetService.cs`
- Test: `backend/tests/Echo.Auth.Tests/TokenValidityTests.cs`

**Interfaces:**
- Produces: `internal static class TokenValidity { internal static string? Problem(DateTime expiresAt, DateTime? usedAt, DateTime? invalidatedAt, DateTime now); }` returning `"token_expired"`, `"token_used"`, `"token_invalidated"` (checked in that order) or `null` when usable. These strings are the existing `auth.result` values.
- `VerifyEmail` and `ResetPassword` replace their separate expired/used/invalidated `if` blocks with one `Problem(...)` call and one failure branch; the per-reason log calls are kept via a `switch` on the returned string.

- [ ] **Step 1: Write failing tests** `Problem_Expired_ReturnsTokenExpired`, `Problem_Used_ReturnsTokenUsed`, `Problem_Invalidated_ReturnsTokenInvalidated`, `Problem_ExpiredAndUsed_ReturnsExpiredFirst`, `Problem_Valid_ReturnsNull`.
- [ ] **Step 2: Run, expect FAIL**; **Step 3: implement and convert both services**; **Step 4: run, expect PASS** plus the existing flow tests.
- [ ] **Step 5: Docs and commit:** update `docs/backend/architecture.md` (instrumentation convention), commit `refactor(auth): one place for token validity`, run the full `dotnet test backend/Echo.slnx`, mark the Phase 3 PR ready.
