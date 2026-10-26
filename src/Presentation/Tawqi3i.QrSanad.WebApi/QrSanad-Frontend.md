# QrSanad API: frontend guide

Everything the frontend needs to call the QrSanad API: authentication, the response format, every
endpoint, and how to handle errors. The live, always-current contract is Swagger:
`{baseUrl}/swagger` (development only).

- **Base URL (local):** `http://localhost:5080/api/v1`
- **Allowed dev origins (CORS):** `http://localhost:3000`, `http://localhost:4200`, `http://localhost:5173`

---

## 1. Concepts

### User types

| `type` | Who | What they can do |
|---|---|---|
| `superAdmin` | One system account | Manages branches and admins, sees everything, holds every permission |
| `admin` | One per branch | Manages the users of **their own branch** |
| `user` | Belongs to one branch | Only what their admin granted (permissions) |

- Admins and users only ever see data from their own branch. Anything outside it returns `404`.
- When a branch is disabled, its admin and users are locked out on their next request (`401`), and their login returns `403`.

### Permissions

The UI should show or hide features based on the `permissions` array from `GET /auth/me`. The
server always enforces them anyway.

| Code | Allows |
|---|---|
| `users.view` | List and view users of the branch |
| `users.create` | Create users |
| `users.update` | Edit users, activate or disable them, unlock them |
| `users.delete` | Delete users |
| `users.reset_password` | Set another user's password |
| `users.assign_permissions` | Change a user's permissions |
| `permissions.view` | Read the permission catalogue |
| `audit_logs.view` | Read the security audit log |
| `eseal.view` | Retrieve e-sealed documents by SANAD QR code (see `ESeal-Frontend.md`; the identity card holder in `IdentityCard-Frontend.md`) |

Branch management doesn't use a permission: only `type === "superAdmin"` can manage branches.

---

## 2. Request and response format

### Headers

```
Content-Type: application/json
Authorization: Bearer <accessToken>     (every endpoint except the anonymous ones)
Accept-Language: ar | en                (error and success messages come back in that language)
```

### Success

```json
{ "message": "Created successfully", "data": { ... } }
```

`message` is omitted when there is none. `data` is omitted for actions with no payload.

### Paged lists

All lists accept `pageNumber` (default 1) and `pageSize` (default 20, max 100) in the query string.

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 42,
  "pageCount": 3,
  "items": [ ... ]
}
```

### Errors

Always this shape, with the HTTP status carrying the meaning:

```json
{ "message": "Invalid username or password" }
```

Validation errors (`400`) also list each field:

```json
{
  "message": "One or more fields are invalid",
  "errors": {
    "Email": ["The Email field is not a valid email address"],
    "UserName": ["Username must be 3 to 64 characters: letters, digits, dot, underscore or dash"]
  }
}
```

Error keys are the request property names in PascalCase (`Email`, `DisplayName`, …).

### Formats

- **Ids** are GUID strings.
- **Dates** are ISO-8601 in UTC, ending in `Z` (e.g. `2026-10-22T10:15:30.1234567Z`). Convert to local time for display.
- **Enums** are camelCase strings: `"active"`, `"disabled"`, `"superAdmin"`, `"admin"`, `"user"`.

### Status codes

| Code | Meaning | What the UI should do |
|---|---|---|
| 200 / 201 | OK / created | |
| 400 | Validation or business rule failed | Show `message` and the field `errors` |
| 401 | Not signed in, or the token is expired or revoked | Refresh once (see §3.3); if that fails, go to login |
| 403 | Signed in but not allowed. Also: password change pending, account or branch disabled | Show `message` |
| 404 | Not found, or outside your branch | Show not found |
| 409 | Conflict: duplicate name or email, branch already has an admin, record changed by someone else | Show `message` |
| 423 | Account temporarily locked (too many wrong passwords) | Show `message`, suggest waiting 15 minutes |
| 429 | Too many requests | Wait the number of seconds in the `Retry-After` header |
| 500 | Server error | Generic error message |

---

## 3. Authentication

### 3.1 Tokens

| Token | Lifetime | Use |
|---|---|---|
| `accessToken` | 15 minutes | `Authorization: Bearer ...` on every request |
| `refreshToken` | Single use. The session expires 7 days after login, even if you keep refreshing | Exchange for a new pair at `/auth/refresh` |

Storage:
- Keep the **access token in memory** only.
- Keep the **refresh token** in the most protected storage available (secure storage on mobile, `sessionStorage` in a SPA). Never put tokens in URLs or logs.

### 3.2 Login: `POST /auth/login` (anonymous)

```json
{ "userNameOrEmail": "admin.amman", "password": "..." }
```

`200`:

```json
{
  "data": {
    "tokenType": "Bearer",
    "accessToken": "eyJ...",
    "accessTokenExpiresAt": "2026-10-22T10:30:00Z",
    "refreshToken": "kq3...",
    "refreshTokenExpiresAt": "2026-10-29T10:15:00Z",
    "mustChangePassword": false
  }
}
```

| Status | `message` | |
|---|---|---|
| 401 | Invalid username or password | Same response for an unknown user and a wrong password, by design |
| 403 | Account is disabled / The branch is disabled | |
| 423 | Account is temporarily locked. Try again later | Sent after 5 wrong attempts; lasts 15 minutes |
| 429 | Too many requests. Try again later | 10 login or refresh calls per minute per IP |

**If `mustChangePassword` is `true`**, send the user straight to a change-password screen.
Until the password is changed, every feature endpoint returns `403` with
`"You must change your password before continuing"`. Only `/auth/me`, `/auth/password` and the
logout endpoints work.

### 3.3 Refresh: `POST /auth/refresh` (anonymous)

```json
{ "refreshToken": "kq3..." }
```

`200` returns a **new** `accessToken` and a **new** `refreshToken`. Replace both. `401` means
the session is over: go to the login screen.

> ⚠️ **Refresh only once at a time.** Every refresh token works exactly once. If the same refresh
> token is sent twice (two tabs, or two requests that got `401` at the same moment), the server
> treats it as theft and **signs the user out on every device**. Use one shared in-flight refresh:

```ts
let refreshing: Promise<boolean> | null = null;

async function refreshOnce(): Promise<boolean> {
  if (!refreshing) {
    refreshing = (async () => {
      const res = await fetch(`${baseUrl}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: tokens.refreshToken }),
      });
      if (!res.ok) return false;
      const { data } = await res.json();
      tokens = data;              // replace BOTH tokens
      return true;
    })().finally(() => (refreshing = null));
  }
  return refreshing;
}

// In your HTTP client: on 401 (not from /auth/login or /auth/refresh),
// await refreshOnce(); if true, retry the original request once; otherwise go to login.
```

You can also refresh shortly before `accessTokenExpiresAt` instead of waiting for a `401`. For
multiple tabs, coordinate with a `BroadcastChannel` or storage events, so that only one tab
refreshes.

### 3.4 Current user: `GET /auth/me`

Call this after login and on app start, to build the menu and permissions.

```json
{
  "data": {
    "id": "0199...",
    "userName": "admin.amman",
    "email": "admin.amman@qrsanad.local",
    "displayName": "Amman Admin",
    "phoneNumber": null,
    "type": "admin",
    "branchId": "0199...",
    "branchName": "Amman Branch",
    "mustChangePassword": false,
    "lastLoginAt": "2026-10-22T10:15:00Z",
    "passwordChangedAt": "2026-10-20T08:00:00Z",
    "permissions": ["audit_logs.view", "users.create", "users.view"]
  }
}
```

The SuperAdmin has `type: "superAdmin"`, `branchId: null`, and every permission.

### 3.5 Change own password: `PUT /auth/password`

```json
{ "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }
```

`200` returns a new token pair, in the same shape as login, with
`message: "Password changed successfully"`. **Replace both tokens.** Every other session of
the user ends.

`400` messages: `Current password is incorrect`,
`Password does not meet the password policy`,
`New password must be different from the current password`. A mismatched confirmation arrives as
a validation error: `errors.ConfirmPassword: ["Passwords do not match"]`.

### 3.6 Password rules: `GET /auth/password-policy` (anonymous)

Use this to show the rules as the user types:

```json
{ "data": { "minLength": 10, "maxLength": 128, "requireUppercase": true, "requireLowercase": true, "requireDigit": true, "requireNonAlphanumeric": true } }
```

The SuperAdmin can change the policy with `PUT /auth/password-policy` (see `PasswordPolicy-Frontend.md`).
Don't cache it for long: fetch it when a password screen opens.

### 3.7 Logout

| Endpoint | Body | Effect |
|---|---|---|
| `POST /auth/logout` | `{ "refreshToken": "..." }` | Ends this session. Then delete both tokens locally |
| `POST /auth/logout-all` | none | Ends every session on every device, including this one |

---

## 4. Branches

All branch endpoints require **`superAdmin`**, except `GET /branches/current`.

### Branch object

```json
{
  "id": "0199...",
  "name": "Amman Branch",
  "code": "AMM",
  "address": "Amman",
  "phoneNumber": null,
  "status": "active",
  "admin": { "id": "0199...", "userName": "admin.amman", "displayName": "Amman Admin", "status": "active" },
  "usersCount": 4,
  "createdAt": "2026-10-22T10:00:00Z",
  "updatedAt": null
}
```

`admin` is `null` until an admin has been created for the branch.

| Method & route | Body / query | Result |
|---|---|---|
| `GET /branches` | `?search=&status=active\|disabled&pageNumber=&pageSize=` | Paged list of branches |
| `GET /branches/{id}` | | Branch |
| `GET /branches/current` | | The caller's own branch (admins and users) |
| `POST /branches` | `{ "name": "...", "code": "AMM", "address": "...", "phoneNumber": "..." }` | `201` + branch |
| `PUT /branches/{id}` | Same body as create | Branch |
| `PUT /branches/{id}/status` | `{ "status": "disabled" }` or `"active"` | Disabling locks out the branch's admin and users at once |
| `DELETE /branches/{id}` | | Soft delete. Its admin and users can no longer sign in |

Field rules:
- `name`: required, 2–128 characters, unique.
- `code`: optional, 1–32 of `A-Z a-z 0-9 _ -`, unique.
- `address`: up to 256 characters.
- `phoneNumber`: up to 32 characters.

`409` messages: `Branch name is already in use`, `Branch code is already in use`.

---

## 5. Users

### User object

```json
{
  "id": "0199...",
  "userName": "user.one",
  "email": "user.one@qrsanad.local",
  "displayName": "User One",
  "phoneNumber": null,
  "type": "user",
  "branchId": "0199...",
  "branchName": "Amman Branch",
  "status": "active",
  "isLocked": false,
  "lockoutEnd": null,
  "mustChangePassword": true,
  "lastLoginAt": null,
  "createdAt": "2026-10-22T10:20:00Z",
  "updatedAt": null
}
```

The details endpoints (`GET /users/{id}`, create, update, set permissions) add
`"permissions": ["users.view", ...]`.

### Endpoints

| Method & route | Permission | Body / query |
|---|---|---|
| `GET /users` | `users.view` | `?search=&status=&type=&branchId=&pageNumber=&pageSize=`. `branchId` only has an effect for the SuperAdmin |
| `GET /users/{id}` | `users.view` | |
| `POST /users` | `users.create` | See below |
| `PUT /users/{id}` | `users.update` | `{ "email": "...", "displayName": "...", "phoneNumber": "..." }` |
| `PUT /users/{id}/status` | `users.update` | `{ "status": "active" \| "disabled" }`. Disabling signs the user out everywhere |
| `PUT /users/{id}/unlock` | `users.update` | No body. Clears a lockout |
| `PUT /users/{id}/password` | `users.reset_password` | `{ "newPassword": "...", "confirmPassword": "...", "mustChangePassword": true }` |
| `PUT /users/{id}/permissions` | `users.assign_permissions` | `{ "permissions": ["users.view", ...] }`. This is the **complete** new set |
| `DELETE /users/{id}` | `users.delete` | Soft delete; signs the user out everywhere |

### Creating users: `POST /users`

```json
{
  "userName": "user.one",
  "email": "user.one@qrsanad.local",
  "displayName": "User One",
  "phoneNumber": "+962790000000",
  "password": "Initial!Pass2026",
  "mustChangePassword": true,
  "type": "user",
  "branchId": null,
  "permissions": ["users.view"]
}
```

| Caller | `type` | `branchId` |
|---|---|---|
| SuperAdmin creating a **branch admin** | `"admin"` | **required** |
| SuperAdmin creating a user | `"user"` (default) | **required** |
| Admin creating a user | omit, or `"user"` | omit. It's always the admin's own branch |

- A new admin with an empty `permissions` list gets every permission.
- You can only grant permissions you hold yourself; otherwise the server returns `403 You cannot grant or revoke permissions you do not have`.
- `mustChangePassword` defaults to `true`: the user picks their own password at first login.

Field rules:
- `userName`: 3–64 of `A-Z a-z 0-9 . _ -` (no `@`), unique.
- `email`: valid and unique.
- `displayName`: 2–128 characters.
- `password`: must meet the password policy.

| Status | `message` |
|---|---|
| 400 | `A branch is required for this type of user`, `The branch is disabled`, `Password does not meet the password policy`, `One or more permissions do not exist` |
| 403 | `You cannot create this type of user`, `You cannot grant or revoke permissions you do not have` |
| 404 | `Branch not found` |
| 409 | `Username is already in use`, `Email is already in use`, `The branch already has an admin` |

### Rules the UI should reflect

- Hide the "edit, disable, delete, reset password, permissions" actions on the current user's own row. The server answers `400 You cannot perform this action on your own account`. (Users change their own password through `/auth/password`.)
- Admins can't manage other admins: `403 You cannot manage this user`.
- When a branch admin is deleted, the SuperAdmin can create a new admin for that branch.

---

## 6. Permission catalogue: `GET /permissions`

Requires `permissions.view`. Use it for the permission checkboxes.

```json
{ "data": [ { "id": "…", "code": "users.view", "description": "View the users of the branch" } ] }
```

Only enable the checkboxes whose code is in the current user's own `permissions` (from `/auth/me`).

---

## 7. Audit log: `GET /audit-logs`

Requires `audit_logs.view`. Admins see their own branch only.

Query parameters:
- `userId`: entries where the user is the subject or the actor.
- `eventType`
- `from`: UTC, inclusive.
- `to`: UTC, exclusive.
- `pageNumber`, `pageSize`

Entries come newest first.

```json
{
  "id": "…",
  "eventType": "loginFailed",
  "userId": "…",
  "userName": "user.one",
  "actorId": null,
  "branchId": "…",
  "ipAddress": "10.0.0.5",
  "userAgent": "Mozilla/5.0 …",
  "details": "Invalid password",
  "createdAt": "2026-10-22T10:25:00Z"
}
```

`eventType` values:
- Authentication: `loginSucceeded`, `loginFailed`, `accountLocked`, `logout`, `logoutAll`, `refreshTokenReuseDetected`, `passwordChanged`, `passwordReset`
- Users: `userCreated`, `userUpdated`, `userStatusChanged`, `userUnlocked`, `userDeleted`, `userPermissionsChanged`
- Branches: `branchCreated`, `branchUpdated`, `branchStatusChanged`, `branchDeleted`
- QR scans: `qrScanSucceeded`, `qrScanFailed` (see `ESeal-Frontend.md`)
- Settings: `passwordPolicyUpdated` (see `PasswordPolicy-Frontend.md`)

QR scan counts for the dashboard: `GET /qr-sanad/stats` (see `ScanStats-Frontend.md`).
The scans themselves, with search by national number or name: `GET /qr-sanad/scans` (see `Scans-Frontend.md`).

---

## 8. Typical screens and flows

1. **Login**
   - `POST /auth/login`.
   - If `mustChangePassword` is true, go to the change-password screen (`PUT /auth/password`, then replace the tokens).
   - Then call `GET /auth/me` and route by `type`.
2. **SuperAdmin: set up a branch**
   - `POST /branches`.
   - `POST /users` with `type: "admin"` and the new `branchId`.
   - Give the admin their temporary password; they must change it at first login.
3. **Admin: add a user**
   - `GET /permissions` for the checkboxes.
   - `POST /users` with no `type` or `branchId`.
4. **Disable or enable a branch:** `PUT /branches/{id}/status`.
5. **Locked user:** an admin calls `PUT /users/{id}/unlock`.
6. **Forgotten password:** an admin calls `PUT /users/{id}/password`. There is no self-service reset by email yet.
