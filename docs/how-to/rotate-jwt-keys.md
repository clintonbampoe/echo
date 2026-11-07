# JWT Key Rotation

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## How It Works

API signs with one RSA key, validates against **two**: active + previous (grace period).
Each token carries `kid` (key fingerprint) so API knows which key to verify.

## Planned Rotation (Graceful)

Access tokens: 15 min. Grace period: few hours to a day.

1. **Generate new pair, keep old public key:**
   ```bash
   sh backend/tools/jwt-key-setup/SetupJwtKeys.sh env --rotate
   ```
   Moves current `Jwt__PublicKey` → `Jwt__PreviousPublicKey`, writes fresh pair.

2. **Deploy.** New logins get new key. Old tokens still work (old key on ring).

3. **Wait out grace period** (> 15 min). No valid old tokens remain.

4. **Clear `Jwt__PreviousPublicKey` and deploy again.** Old key out of circulation.

> **Never skip step 4.** Retired key on ring indefinitely = still validates tokens.

## Emergency Rotation (Key Compromise)

Skip grace period — forced logout is correct.

1. **Generate new pair WITHOUT keeping old public key:**
   ```bash
   sh backend/tools/jwt-key-setup/SetupJwtKeys.sh env
   ```
   Confirm `Jwt__PreviousPublicKey` is empty.

2. **Deploy.** All old tokens rejected immediately.

3. **Refresh tokens** are random values hashed in DB — not signed by JWT key. Rotation doesn't revoke them. To fully log out, revoke refresh tokens too.

## Verification

- Startup fails fast if `Jwt__PublicKey` ≠ public half of `Jwt__PrivateKey`, or `Jwt__PreviousPublicKey` = active key.
- Tests: `backend/tests/Echo.Auth.Tests/Services/AccessTokenRotationTests.cs`
- Manual: log in before/after deploy, decode tokens — `kid` should differ, pre-deploy token works until step 4.