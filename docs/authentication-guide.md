# Authentication and Authorization Guide

This document summarizes how JWT-based authentication is configured in the Parking API and how to interact with the exposed endpoints that now require a bearer token.

## Configuration Overview

The API registers JWT authentication in `Program.cs`, wiring the `JwtBearer` handler with issuer, audience, signing key, and a zero clock skew so tokens expire exactly at the configured time.【F:src/Parking.Api/Program.cs†L1-L66】  The values come from the `Jwt` section of `appsettings.json`, where you can adjust the issuer, audience, secret key, and access token lifetime (in minutes).【F:src/Parking.Api/appsettings.json†L1-L18】

User accounts are stored in the new `Users` table. Entity Framework seeds a default administrator with the email `admin@parking.local` so that the system can be accessed immediately after provisioning.【F:src/Parking.Infrastructure/Persistence/Configurations/UserConfiguration.cs†L12-L39】  You can update the seed user or add migrations to provision additional accounts as needed.

## Login Endpoint

Authenticate by sending a `POST` request to `/api/Auth/login` with the user's credentials.【F:src/Parking.Api/Controllers/AuthController.cs†L10-L52】  Both fields are required and validated server-side.【F:src/Parking.Api/Models/Requests/LoginRequest.cs†L1-L13】

### Request Example

```http
POST /api/Auth/login HTTP/1.1
Host: localhost:5001
Content-Type: application/json

{
  "email": "admin@parking.local",
  "password": "YourAdminPasswordHere"
}
```

> Replace `YourAdminPasswordHere` with the actual password for the seeded or provisioned user.

### Response Example

A successful login returns the access token, its expiration timestamp, and the authenticated user's profile information.【F:src/Parking.Api/Controllers/AuthController.cs†L33-L49】【F:src/Parking.Api/Models/Responses/LoginResponse.cs†L1-L9】

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-05-22T15:04:05.1234567+00:00",
  "user": {
    "id": "8a2c929f-3a3f-4f67-9d92-56977d042793",
    "name": "Administrador",
    "email": "admin@parking.local",
    "role": "Admin"
  }
}
```

## Calling Protected Endpoints

Controllers that manage tickets, vehicle inspections, and admin dashboards now require a valid bearer token via the `[Authorize]` attribute.【F:src/Parking.Api/Controllers/TicketsController.cs†L10-L82】【F:src/Parking.Api/Controllers/AdminDashboardController.cs†L10-L48】  Include the `Authorization` header in subsequent requests:

```http
Authorization: Bearer {accessToken}
```

Example curl command to fetch active tickets after authenticating:

```bash
curl -H "Authorization: Bearer $TOKEN" \
     https://localhost:5001/api/tickets
```

Replace `$TOKEN` with the `accessToken` returned by the login endpoint. When the token expires (based on the configured expiration window), repeat the login process to obtain a new one.

