# E-Seal: QR scans in the audit log

Every call to `POST /qr-sanad/sealed-doc` is now written to the audit log (`GET /audit-logs`), whether it succeeds or fails, together with the user who scanned.

## New event types

| `eventType` | When | `details` |
|---|---|---|
| `qrScanSucceeded` | The sealed document was returned | `QR: <qr code>` |
| `qrScanFailed` | MoDEE rejected the QR code (400) or was unavailable (502) | `<status> <message> \| QR: <qr code>` |

## Who did it

- `userId` and `userName`: the user who scanned.
- `branchId`: their branch.
- `ipAddress` and `userAgent`: the device the request came from.
- `actorId`: always `null` for scans.

Admins see the scans of their own branch; the SuperAdmin sees all of them.

Validation errors, `401`, and `403` for a missing `eseal.view` permission are rejected before the scan, so they are not logged.

## Filters

- All successful scans: `?eventType=qrScanSucceeded`
- All failed scans: `?eventType=qrScanFailed`
- One user's scans: `?userId=<id>&eventType=qrScanSucceeded`

## Frontend changes

- `src/features/audit/types.ts`: add a group, e.g. `scans: ['qrScanSucceeded', 'qrScanFailed']`
- `src/features/audit/eventMeta.ts`: an icon and tone for each (`qrScanFailed` as `danger`)
- `src/lib/i18n/locales/en.ts` and `ar.ts`:

| Event | English | Arabic |
|---|---|---|
| `qrScanSucceeded` | QR scanned | مسح رمز QR |
| `qrScanFailed` | QR scan failed | فشل مسح رمز QR |
