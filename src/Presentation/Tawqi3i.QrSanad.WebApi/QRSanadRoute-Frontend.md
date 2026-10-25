# QR Sanad: new route

The QR Sanad endpoints moved from `/eseal/...` to `/qr-sanad/...`. Requests, responses, errors and permissions are unchanged.

| Before | After |
|---|---|
| `POST /eseal/sealed-doc` | `POST /qr-sanad/sealed-doc` |
| `GET /eseal/stats` | `GET /qr-sanad/stats` |

**The old paths return `404` once the backend is deployed**, so ship this together with the backend.

## Lines to change

- `src/features/eseal/api.ts:7`: `'/eseal/sealed-doc'` → `'/qr-sanad/sealed-doc'`
- `src/features/dashboard/scanStats.ts:40`: `'/eseal/stats'` → `'/qr-sanad/stats'`
- `src/features/eseal/components/SealedDocError.tsx:18`: the comment `POST /eseal/sealed-doc` → `POST /qr-sanad/sealed-doc`

## Unchanged

- The permission is still `eseal.view` (`PERMISSIONS.esealView`). Its id is derived from the code, so renaming it would remove it from every user who holds it.
- The `src/features/eseal` folder name is yours to keep or rename.
