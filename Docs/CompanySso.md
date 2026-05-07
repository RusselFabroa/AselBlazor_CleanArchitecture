# AselDev Blazor Company SSO

This project can act as the company Identity app for internal Blazor/API modules.

## Identity App Responsibilities

- Own user accounts, passwords, roles, and active/inactive status.
- Issue JWT access tokens after Employee ID / username login.
- Expose the current user profile through a protected SSO endpoint.
- Keep user registration/admin creation centralized.

## Template Modes

The template supports two SSO modes through `appsettings.json`.

Use this for the main Company DX Portal:

```json
"Sso": {
  "Mode": "IdentityProvider",
  "Authority": "https://company-portal",
  "LoginUrl": "/login",
  "UserInfoUrl": "/api/sso/me"
}
```

Use this for cloned module apps:

```json
"Sso": {
  "Mode": "Client",
  "Authority": "https://company-portal",
  "LoginUrl": "https://company-portal/login",
  "UserInfoUrl": "https://company-portal/api/sso/me"
}
```

In `Client` mode, protected pages redirect to the portal login and local user administration is hidden.

## SSO Endpoints

Base route:

```text
/api/sso
```

Discovery:

```http
GET /api/sso/.well-known
```

Login/token:

```http
POST /api/sso/token
Content-Type: application/json

{
  "usernameOrEmployeeId": "admin",
  "password": "Admin@12345!"
}
```

Current user:

```http
GET /api/sso/me
Authorization: Bearer {access_token}
```

## Token Claims

Other company apps can trust these claims after validating the JWT signature, issuer, audience, and expiry:

```text
sub
nameidentifier
name
email
preferred_username
employee_id
department
role
jti
```

## Other App Setup

Other ASP.NET Core apps should configure JWT bearer auth using the same issuer, audience, and signing key from this Identity app:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "AselDevBlazor",
            ValidAudience = "AselDevBlazorUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("same-signing-key"))
        };
    });
```

Then protect controllers/pages with:

```csharp
[Authorize]
```

or:

```csharp
[Authorize(Roles = "Admin")]
```
