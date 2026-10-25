# Licensing

The API now runs under a license: a customer, a type, an expiry date, and optional limits on users and branches.

## License page (SuperAdmin only)

### `GET /license`

```json
{
  "data": {
    "customer": "Customer name",
    "type": "PROD",
    "issuedAt": "2026-10-24T12:00:00Z",
    "expiresAt": "2027-10-24T00:00:00Z",
    "isExpired": false,
    "daysRemaining": 364,
    "maxUsers": 50,
    "usersCount": 12,
    "maxBranches": 5,
    "branchesCount": 3
  }
}
```

- `type`: `TRIAL`, `PROD` or `DEV`.
- `maxUsers` / `maxBranches`: `null` means unlimited. `usersCount` does not include the SuperAdmin.
- `404 No valid license is installed` should not happen; the API does not start without one.

### `PUT /license`

```json
{ "key": "QRS-LIC-…" }
```

Installs a new key, e.g. a renewal. `200` returns the license as above with `message: "License updated successfully"`.

| Status | `message` |
|---|---|
| 400 | `The license key is invalid` |
| 400 | `The license has expired. Contact your provider to renew it` (the key itself is expired) |
| 400 | `One or more fields are invalid`, with `errors.Key` (missing, or over 2000 characters) |

Suggested UI: a card with the customer, type and expiry; usage bars for users and branches; a warning when `daysRemaining` is under 30; and a "Install license key" dialog with a textarea.

## New errors elsewhere

| Endpoint | Status | `message` |
|---|---|---|
| `POST /qr-sanad/sealed-doc` | 403 | `The license has expired. Contact your provider to renew it` |
| `POST /users` | 403 | `The license has expired. Contact your provider to renew it` or `The license user limit has been reached` |
| `POST /branches` | 403 | `The license has expired. Contact your provider to renew it` or `The license branch limit has been reached` |

These `403`s come with a localised `message`; show it as is. They are not permission problems, so don't hide the button because of them.

## Audit log

Installing a key is logged as `licenseUpdated` (SuperAdmin only, no branch), with the license summary in `details`.

Add it to `src/features/audit/types.ts`, `src/features/audit/eventMeta.ts`, and the locales:

| Event | English | Arabic |
|---|---|---|
| `licenseUpdated` | License updated | تحديث الترخيص |
