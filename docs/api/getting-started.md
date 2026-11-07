# Getting Started with Echo API

This is your entry point if you are setting up Echo locally for the first time. The main [API Usage](./api-usage.md) guide assumes you already have a valid JWT token. 

Echo is a multi-tenant system. This means that every user (including Admins) must belong to a Congregation. This guide walks you through the mandatory two-step process to create your first valid credentials.

---

## Creating a new user account

**Admins cannot exist alone.** The `User` table has a required foreign key (`CongregationId`). A `User` account must be attached to a congregation.

There are two ways of creating an account in **Echo**:

1. **Registering as an admin of a new congregation**: The backend handles the creation of both the Admin account and the Congregation in a single database transaction.
2. **Signing up to an existing congregation**: If you have an **Invitation Token** from an existing admin, you can use it to join that specific congregation.

### Registering as a new congregation admin

**Endpoint:** `POST /api/v1/auth/registrations/congregation`
_This is a public endpoint. No authentication required_

**Payload**
```json
{
  "congregationDto": {
    "name": "Grace Chapel",
    "orgType": "Church",
    "phoneNumber": "+233...",
    "emailAddress": "info@gracechapel.com",
    "postalAddress": "...",
    "websiteUrl": "https://gracechapel.com",
    "region": "Ahafo",
    "city": "...",
    "town": "...",
    "gpsAddress": "..."
  },
  "userDto": {
    "name": "Clinton Bampoe",
    "emailAddress": "clintbamp@gmail.com",
    "password": "SecurePassword123!",
    "role": "Admin"
  }
}
```

**Response (200 OK)** — you are signed in straight away, same shape as login:
```json
{
  "accessToken": "eyJ...",
  "accessTokenExpiresAt": "2026-10-31T12:15:00Z",
  "refreshToken": "...",
  "refreshTokenExpiresAt": "2026-11-30T12:00:00Z"
}
```
> **Note:** The account is not verified yet; a verification email is queued and arrives shortly. Verification is not needed to use the session.

### Signing up with an invitation token

**Endpoint**: `POST /api/v1/auth/registrations/member`

**Payload**
```json
{
  "token": "INVITE_TOKEN_HERE",
  "name": "John Doe",
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response (200 OK)** — you are signed in straight away, same token pair as above (`accessToken`, `accessTokenExpiresAt`, `refreshToken`, `refreshTokenExpiresAt`).

> **Note:** The `Role` of the account created is determined by the type of **Invitation Token** provided.

---

## Verifying your Account

Verifying your email is not needed to log in. The app reminds you until you do (it asks `GET /api/v1/auth/verifications/status`, which is only for signed-in users and says `verified: true` or `false`). 

**Endpoint**: `POST /api/v1/auth/verifications/account`

**Payload**
```json
{
  "email": "clintbamp@gmail.com"
}
```

### Where the token comes from
The verification token is only sent by email (Resend); the response never contains it. Until the sending domain is verified in Resend, mail can only reach the account's own address, so use that address when testing.

**Response (200 OK)**
```json
{
  "message": "Email has been sent if user is valid"
}
```

### Completing Verification
Take the token from the email and pass it to the verify-email endpoint.

**Endpoint**: `GET /api/v1/auth/verifications/verify-email?token=####`
(Replace `####` with your token)

**Response (200 OK)**
```json
{
  "data": {
    "message": "Account verified successfully."
  }
}
```

---

## Authenticating and acquiring a JWT token

Once you have a `User` account (verified or not), you can acquire a JWT token to access protected endpoints.

See [Acquiring a JWT Token](./api-usage.md#authentication) for more details.
