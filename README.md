# Tawqi3i.Application.WebServices.QrSanad

.NET 10 Web API for QrSanad's users, branches and permissions. It follows the layering of
`Tawqi3i.Application.WebServices`, and runs on SQL Server (default), PostgreSQL, Oracle or MySQL.

## Solution layout

| Project | Role |
|---|---|
| `Tawqi3i.QrSanad.Common` | Entities, DTOs, view models, repository and service contracts, constants |
| `Tawqi3i.QrSanad.Persistence` | `QrSanadSqlContext`, repositories, unit of work, provider switch, seeding, and the migrations of every provider (`Migrations/<Provider>`) |
| `Tawqi3i.QrSanad.Identity` | JWT, password hashing, `[HasPermission]` / `[SuperAdminOnly]`, `AuthService` |
| `Tawqi3i.QrSanad.Sanad` | Client for the MoDEE (Sanad) e-seal service |
| `Tawqi3i.QrSanad.Licensing` | License key verification and limits (see [Licensing](#licensing)) |
| `Tawqi3i.QrSanad.Domain` | Branch, user, permission and audit services; hierarchy rules (`AccessGuard`) |
| `Tawqi3i.QrSanad.WebApi.Common` | Response localisation, Swagger, CORS, logging, security headers |
| `Tawqi3i.QrSanad.WebApi` | The host: controllers, middleware, startup |
| `Tawqi3i.QrSanad.DomainTests` | NUnit tests for the hierarchy rules and security primitives |
| `tools/Tawqi3i.QrSanad.LicenseGenerator` | Issues license keys; never deployed |
| `Database/` | Generated SQL scripts for every provider |

Package versions are pinned centrally in `Directory.Packages.props`; the target framework and
compiler settings are set in `Directory.Build.props`.

## Who can do what

| Level | Can |
|---|---|
| **SuperAdmin** | Exactly one, created at first startup. Creates and manages branches and their admins (`/users`), and the devices of every branch. Holds every permission. |
| **Admin** | Belongs to one branch. Manages that branch's devices (`/devices`) and sees its audit log and scans. Gets every permission by default; the SuperAdmin can narrow that. |
| **Device** | Belongs to one branch. The only type that can scan a QR code. Has no permissions and sees only its own scans. |

Rules enforced by the API:

- Each branch has at most one admin, and each admin belongs to exactly one branch. A unique index on `Branches.AdminUserId` enforces this. Deleting the admin frees the branch for a new one.
- `/users` is the SuperAdmin's alone and lists everyone except devices. `/devices` lists devices only.
- Outside the SuperAdmin, everyone sees only their own branch. Devices in other branches return `404`, so their existence can't be confirmed.
- Disabling a branch locks out its admin and devices on their next request. Activating it lets them back in. Deleting a branch (soft delete) also ends their sessions.
- Nobody can disable, delete, reset or re-permission their own account.
- Signing in ends every other session of that account.

Permissions are listed in `Common/Constants/PermissionCodes.cs` and seeded by the migrations.
To add a permission, add a code there and add a migration for each provider (see below).

## Running locally

Requires the .NET 10 SDK. Development uses SQL Server LocalDB, and migrations are applied at startup.

```bash
cd Tawqi3i.Application.WebServices.QrSanad
dotnet tool restore

# Secrets are never committed. Set them once per machine:
dotnet user-secrets set "Identity:Jwt:SecretKey" "<at least 32 random bytes, e.g. base64 of 48 bytes>" --project src/Presentation/Tawqi3i.QrSanad.WebApi
dotnet user-secrets set "Persistence:Seed:Password" "<initial SuperAdmin password>" --project src/Presentation/Tawqi3i.QrSanad.WebApi

dotnet run --project src/Presentation/Tawqi3i.QrSanad.WebApi --launch-profile http
```

- Swagger: http://localhost:5080/swagger
- Health: http://localhost:5080/health
- The SuperAdmin logs in as `superadmin` and has to change the password first. Until it is changed, every permission-protected endpoint returns `403`.

## Switching database

Set `Persistence:Sql:Provider` and `Persistence:Sql:DefaultConnection`, normally through environment
variables in deployed environments:

| Provider | Example connection string | Naming |
|---|---|---|
| `SqlServer` (default) | `Server=.;Database=QrSanadDb;User Id=...;Password=...;TrustServerCertificate=True` | PascalCase |
| `PostgreSql` | `Host=localhost;Port=5432;Database=qrsanad;Username=...;Password=...` | snake_case |
| `Oracle` | `User Id=QRSANAD;Password=...;Data Source=host:1521/SERVICE` (also set `OracleSqlCompatibility`: 19, 21 or 23) | UPPER_SNAKE_CASE |
| `MySql` | `Server=localhost;Port=3306;Database=qrsanad;User=...;Password=...` | snake_case |

```bash
# example: PostgreSQL
export Persistence__Sql__Provider=PostgreSql
export Persistence__Sql__DefaultConnection="Host=...;Database=qrsanad;Username=...;Password=..."
```

MySQL uses Oracle's `MySql.EntityFrameworkCore`, because Pomelo has no EF Core 10 release yet.

## Database scripts and migrations

`Database/<Provider>/QrSanad.<Provider>.sql` is the full schema plus seed data. The scripts are
idempotent, so they're safe to run again. For SQL Server, run `00_CreateDatabase.sql` first. In
production, prefer running these scripts as a deployment step and keeping
`ApplyMigrationsOnStartup` off.

There is one Persistence project. Each provider has a thin context in `Sql/Providers`
(`SqlServerQrSanadContext`, `PostgreSqlQrSanadContext`, `OracleQrSanadContext`, `MySqlQrSanadContext`)
that owns its migrations in `Migrations/<Provider>`. At runtime, `Persistence:Sql:Provider`
selects the context, and the rest of the code sees only `QrSanadSqlContext`.

After changing an entity or a configuration, add a migration for **every** provider and regenerate the scripts:

```bash
P=src/Infrastructure/Tawqi3i.QrSanad.Persistence
for p in SqlServer PostgreSql Oracle MySql; do
  dotnet ef migrations add <Name> --project $P --startup-project $P --context ${p}QrSanadContext \
    --output-dir Migrations/$p --namespace Tawqi3i.QrSanad.Persistence.Migrations.$p
  dotnet ef migrations script --idempotent --project $P --startup-project $P --context ${p}QrSanadContext \
    -o Database/$p/QrSanad.$p.sql
done
```

`migrations add` and `migrations script` never connect to a database. `database update`
connects to whatever `QRSANAD_CONNECTION` points at. After regenerating the SQL Server script,
put back the `SET QUOTED_IDENTIFIER ON` header: `sqlcmd` needs it for the filtered unique indexes.

## Endpoints

The frontend contract (request and response shapes, error handling, the token refresh flow) is
in `src/Presentation/Tawqi3i.QrSanad.WebApi/QrSanad-Frontend.md`.

| Method | Route | Access |
|---|---|---|
| POST | `/api/v1/auth/login`, `/api/v1/auth/refresh` | Anonymous, rate limited |
| POST | `/api/v1/auth/logout`, `/api/v1/auth/logout-all` | Signed in |
| GET | `/api/v1/auth/me` | Signed in |
| PUT | `/api/v1/auth/password` | Signed in |
| GET | `/api/v1/auth/password-policy` | Anonymous |
| PUT | `/api/v1/auth/password-policy` | SuperAdmin |
| GET/POST/PUT/DELETE | `/api/v1/branches`, `/{id}`, `/{id}/status` | SuperAdmin |
| GET | `/api/v1/branches/current` | Signed in |
| GET/POST/PUT/DELETE | `/api/v1/users`, `/{id}` | `users.*` permissions, scoped to the caller's branch |
| PUT | `/api/v1/users/{id}/status`, `/unlock`, `/password`, `/permissions` | `users.update`, `users.reset_password`, `users.assign_permissions` |
| GET | `/api/v1/permissions` | `permissions.view` |
| GET | `/api/v1/audit-logs` | `audit_logs.view`, scoped to the caller's branch |
| POST | `/api/v1/qr-sanad/sealed-doc` | `eseal.view`; calls the MoDEE e-seal service directly (`Sanad` settings) |
| GET | `/api/v1/qr-sanad/stats` | `audit_logs.view`, scoped to the caller's branch |
| GET/PUT | `/api/v1/license` | SuperAdmin |

The SuperAdmin creates an admin with `POST /api/v1/users` and
`{ "type": "admin", "branchId": "...", ... }`. An admin creates users with the same endpoint,
without `type` or `branchId`.

Responses use the shape `{ "message": "...", "data": ... }`. Messages are in Arabic when the
request sends `Accept-Language: ar`.

## Licensing

The API needs a license key issued with `tools/Tawqi3i.QrSanad.LicenseGenerator` (see its README).
A license names the customer and its type (TRIAL, PROD, DEV), and sets an expiry date and
optional limits on users (admins), devices and branches. The key is signed with ECDSA P-256 and checked against the
public key built into the API, so it cannot be edited.

- **Startup:** without a valid key in `Licensing:Key` or the database, the API refuses to start. An
  expired license still starts, so the SuperAdmin can sign in and install a renewal.
- **Expired:** QR scans, new admins, new devices and new branches are refused with `403`. Everything else keeps working.
- **Limits:** creating an admin past `maxUsers` (the SuperAdmin is not counted), a device past
  `maxDevices`, or a branch past `maxBranches`, is refused with `403`. A key without `maxDevices`
  allows unlimited devices.
- **Renewal:** the SuperAdmin installs a key with `PUT /api/v1/license`, or it is set in
  `Licensing:Key`. At startup, the most recently issued license wins, even if it grants less. Every change is audited as `licenseUpdated`.

`appsettings.Development.json` carries a DEV license, unlimited, until 2031.

## Security

- **Passwords** are hashed with ASP.NET Core Identity's PBKDF2 (HMAC-SHA512, 100k iterations), and old hashes are upgraded at login. The SuperAdmin sets the password policy with `PUT /api/v1/auth/password-policy`, which also requires every user to change their password; `Identity:PasswordPolicy` only supplies the initial values, stored at first startup.
- **Access tokens** expire after 15 minutes and carry no roles or permissions. Permissions are checked per request against a cached snapshot, so revoking access takes effect at once.
- **Refresh tokens** are random, stored only as SHA-256 hashes, and single use. Replaying a token that was already used ends all of that user's sessions.
- **Security stamp:** changing a password, a reset, disabling, deleting or logout-all rotates it, which invalidates every outstanding access and refresh token.
- **Lockout:** 5 failed attempts lock the account for 15 minutes. The counter is updated atomically in SQL.
- **Login** gives the same response and takes the same time whether or not the username exists.
- **Rate limiting:** 10 login/refresh calls per minute per IP, and 300 requests per minute per IP overall.
- **HTTP:** every endpoint requires authentication unless marked `[AllowAnonymous]`. Responses send security headers, HSTS and HTTPS redirection outside Development, and no `Server` header. CORS allows only the configured origins.
- **Audit log:** every login, lockout, QR scan and change to branches, users or permissions is written to `SecurityAuditLogs`.
- **Configuration:** secrets live only in user-secrets or environment variables, and the configuration is never logged.

When running more than one instance, set `Persistence:RedisCache:DefaultConnection`, so that
disabling a user or branch reaches every instance at once. Behind a reverse proxy, list the
proxy in `Security:KnownProxies`, so that rate limits and audit entries see the real client IP.
