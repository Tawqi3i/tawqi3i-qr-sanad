# Password policy: set by the SuperAdmin

The SuperAdmin can now change the password rules from the app. Before, they were fixed in the server configuration.

## `PUT /auth/password-policy`

**SuperAdmin only.** Everyone else gets `403`.

```json
{
  "minLength": 12,
  "maxLength": 128,
  "requireUppercase": true,
  "requireLowercase": true,
  "requireDigit": true,
  "requireNonAlphanumeric": false
}
```

Send all six fields every time; it replaces the whole policy.

| Field | Rules |
|---|---|
| `minLength` | 8 to 128 |
| `maxLength` | 8 to 128, and not less than `minLength` |
| `require*` | `true` / `false` |

`200`: the new policy, in the same shape as `GET /auth/password-policy`, with
`message: "Password policy updated. Every user must change their password"`.

| Status | `message` |
|---|---|
| 400 | `One or more fields are invalid`, with `errors.MinLength` / `errors.MaxLength` (out of range) |
| 400 | `The maximum length must not be less than the minimum length` |
| 403 | `Access is forbidden` (not the SuperAdmin) |

## What changes for users

**Every user must change their password after the policy changes, the SuperAdmin included.**

- Nobody is signed out. Their sessions stay open.
- From their very next request, every feature endpoint returns `403` with
  `"You must change your password before continuing"`. Only `/auth/me`, `PUT /auth/password` and the logout endpoints work.
- `GET /auth/me` returns `mustChangePassword: true`.
- After a successful `PUT /auth/password` (which checks the new rules), everything works again.

What the UI should do:
- On that `403` message, or when `/auth/me` has `mustChangePassword: true`, send the user to the change-password screen. This is the same flow as the first login.
- After the SuperAdmin saves the policy, send them to the change-password screen too.
- `GET /auth/password-policy` (anonymous) returns the current rules. Fetch it when the change-password screen opens, so the hints match the new policy.

## Audit log

Each change is logged as `passwordPolicyUpdated`, with the old and new rules and how many users were flagged in `details`:

```
length 10-128, uppercase True, lowercase True, digit True, symbol True -> length 12-128, uppercase True, lowercase True, digit True, symbol False. 37 user(s) must change their password
```

Each user's own change then appears as `passwordChanged`.

It has no `branchId`, so only the SuperAdmin sees it.

## Frontend changes

- A "Password policy" settings screen, shown only when `type === "superAdmin"`: load with `GET /auth/password-policy`, save with `PUT /auth/password-policy`.
- Audit log screen: add `passwordPolicyUpdated` to `src/features/audit/types.ts`, `src/features/audit/eventMeta.ts`, and the `en.ts` / `ar.ts` locales:

| Event | English | Arabic |
|---|---|---|
| `passwordPolicyUpdated` | Password policy updated | تعديل سياسة كلمة المرور |
