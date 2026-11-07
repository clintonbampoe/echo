# Rotate the JWT keys

Access tokens are signed with an RSA key (RS256). The API signs with one key
and accepts tokens signed by two: the active key and, during a rotation, the
previous one. Each token carries a `kid` (a key fingerprint) so the API knows
which to check.

Refresh tokens are not signed with this key; they are random values stored
hashed in the database. Rotating the key does not revoke them. To log everyone
out, revoke the refresh tokens too.

Startup fails if `Jwt__PublicKey` does not match `Jwt__PrivateKey`, or if
`Jwt__PreviousPublicKey` equals the active key.

## Rotate with a grace period

Use this for routine rotation. Access tokens last 15 minutes, so a grace period
of a few hours is plenty. On the server, as the deploy user:

```bash
cd /srv/echo
cp .env .env.bak
key=$(mktemp); openssl genrsa -out "$key" 2048 2>/dev/null
new_priv=$(base64 -w0 "$key")
new_pub=$(openssl rsa -in "$key" -pubout 2>/dev/null | base64 -w0)
rm "$key"
old_pub=$(grep '^Jwt__PublicKey=' .env | cut -d= -f2-)
sed -i -e '/^Jwt__PreviousPublicKey=/d' \
  -e "s|^Jwt__PrivateKey=.*|Jwt__PrivateKey=$new_priv|" \
  -e "s|^Jwt__PublicKey=.*|Jwt__PublicKey=$new_pub|" .env
echo "Jwt__PreviousPublicKey=$old_pub" >> .env
```

1. **Restart the API** so it reads the new values:

   ```bash
   cd /srv/echo/current
   docker compose -p echo --env-file /srv/echo/.env --env-file release.env \
     -f docker-compose.yml -f docker-compose.prod.yml up -d api
   ```

   New sign-ins get the new key. Tokens signed with the old key still work.
2. **Wait** longer than the token lifetime (15 minutes). No valid old token is
   left after that.
3. **Remove the previous key** and restart the API again:

   ```bash
   sed -i '/^Jwt__PreviousPublicKey=/d' /srv/echo/.env
   ```

   then repeat the `up -d api` command from step 1. Do not skip this: a retired
   key left on the ring keeps validating tokens.

## Rotate at once (key compromise)

Skip the grace period. Everyone is signed out, which is the point. Run the
same commands without keeping the old key: leave out the last `echo` line (the
`sed` already removes `Jwt__PreviousPublicKey`), then restart the API. Every
old token is rejected immediately. Revoke refresh tokens as well if sessions
must end completely.

## On your machine

`backend/tools/jwt-key-setup/SetupJwtKeys.sh` does the same to the `.env` in the
repo root: `sh SetupJwtKeys.sh env --rotate` keeps the old public key as the
previous one, `sh SetupJwtKeys.sh env` does not.

## Check it

- Sign in before and after the restart and decode the tokens: the `kid`
  differs, and the token from before still works until step 3.
- The tests are in
  `backend/tests/Echo.Auth.Tests/Services/AccessTokenRotationTests.cs`.
