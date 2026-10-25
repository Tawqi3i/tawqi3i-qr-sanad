# QR Sanad: MoDEE is now called directly

The backend now calls the MoDEE (Sanad) e-seal service itself, instead of going through ESigner. The endpoints, request, response and permission are unchanged:

- `POST /qr-sanad/sealed-doc` (`eseal.view`)
- `GET /qr-sanad/stats` (`audit_logs.view`)

## Error changes on `POST /qr-sanad/sealed-doc`

| Status | Before | Now |
|---|---|---|
| 403 | `Access is forbidden` (no `eseal.view`), **or** ESigner messages: Sanad QR not enabled, balance used up, credentials not set up | Only `Access is forbidden` (no `eseal.view`) |
| 400 | MoDEE's message for an invalid QR code | Same; when MoDEE gives no message, the localised `The QR code is invalid` |
| 502 | `The e-seal service is unavailable. Try again later` | Same |

What to change:
- Remove the handling of the ESigner `403` messages (balance, plan, credentials). A `403` now only means the user lacks `eseal.view`.
- In the audit log, `qrScanFailed` is now only `400` (rejected QR) or `502` (MoDEE unavailable).
