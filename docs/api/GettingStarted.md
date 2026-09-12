# Getting Started with Echo API

**Written by:** @clintonbampoe
**Last updated:** 2026-09-12 by @clintonbampoe

This is your entry point if you are setting up Echo locally for the first time. The main [Api Usage](./ApiUsage.md) guide assumes you already have a valid JWT token. 

Echo is a multi-tenant system. This means that every user (including Admins) must belong to a Congregation. This guide walks you through the mandatory two-step process to create your first valid credentials.

---

## Creating a new user account

**Admins cannot exist alone.** The `User` table has a required foreign key (`CongregationId`). A `User` account must be attached to a congregation.

There are two ways of creating an account in **Echo**:

1. **Registering as an admin of a new congregation**: The backend handles the creation of both the Admin account and the Congregation in a single database transaction.
2. **Signing up to an existing congregation**: If you have an **Invitation Token** from an existing admin, you can use it to join that specific congregation.

### Registering as a new congregation admin

**Endpoint:** `POST /api/auth/v1/register/congregation`
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

**Response (200 OK)**
```json
{
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66face"
  }
}
```
> **Note:** The `CongregationId` is created but not returned in the response for security reasons.

### Signing up with an invitation token

**Endpoint**: `POST /api/auth/v1/register/member`

**Payload**
```json
{
  "token": "INVITE_TOKEN_HERE",
  "name": "John Doe",
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response (200 OK)**
```json
{
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66face"
  }
}
```
> **Note:** The `Role` of the account created is determined by the type of **Invitation Token** provided.

---

## Verifying your Account

You cannot login until your account is verified. 

**Endpoint**: `POST /api/auth/v1/verify/account`

**Payload**
```json
{
  "email": "clintbamp@gmail.com"
}
```

### ⚠️ Development Workaround: Email Tokens
In production, the verification token is sent via email using Resend. However, because we are using a trial account without a custom domain, emails can only be sent to the registered account (`clintbamp@gmail.com`).

To prevent blocking development for other users, this endpoint returns the token directly in the response during development.

**Response (200 OK)**
```json
{
  "message": "Operation Completed Successfully. Token: 35p7O2pX8RVnzmEK4btktg"
}
```

### Completing Verification
Take the token received above and pass it to the verify-email endpoint.

**Endpoint**: `GET /api/auth/v1/verifications/verify-email?token=####`
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

With your `User` account now verified, you can acquire a JWT token to access protected endpoints.

See [Acquiring a JWT Token](./ApiUsage.md#authentication) for more details.
