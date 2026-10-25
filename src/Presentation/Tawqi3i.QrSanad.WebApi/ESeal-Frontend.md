# E-Seal API: frontend guide

How to fetch the MoDEE e-sealed document for a SANAD QR code. Headers, the response format,
token refresh and the general status codes are the same as in `QrSanad-Frontend.md`.

- **Base URL (local):** `http://localhost:5080/api/v1`
- **Permission:** `eseal.view`

The QrSanad API calls the MoDEE (Sanad) e-seal service directly. The frontend only talks to
QrSanad and never sees Sanad credentials or tokens.

> ⚠️ **Every call goes to MoDEE and is written to the audit log.** Don't call it on page load,
> don't poll it, and don't retry it automatically. Keep the result and reuse it (for example to
> download after viewing).

---

## 1. Permission

| Code | Allows |
|---|---|
| `eseal.view` | Retrieve e-sealed documents by SANAD QR code |

- Show the e-seal screen or button only when `permissions` from `GET /auth/me` contains `eseal.view`.
- The SuperAdmin holds it automatically. Admins and users get it like any other permission: from
  the SuperAdmin or their admin, through `PUT /users/{id}/permissions`.
- Add it to `src/features/permissions/constants.ts`:

```ts
esealView: 'eseal.view',
```

---

## 2. Get a sealed document: `POST /qr-sanad/sealed-doc`

Requires `eseal.view`.

```json
{ "qrCode": "the raw string read from the SANAD QR code" }
```

- `qrCode`: required, up to 4096 characters. Send it exactly as scanned: no trimming, decoding or reformatting.

`200`:

```json
{
  "data": {
    "signedFile": "JVBERi0xLjcKJeLjz9MK...",
    "contentType": "application/pdf"
  }
}
```

| Field | Notes |
|---|---|
| `signedFile` | Base64 file contents (a PAdES-T PDF). Can be several MB |
| `contentType` | Usually `application/pdf` |

The request can take a while (the server waits up to 100 seconds for MoDEE). Show a loading
state and disable the submit button until it finishes.

### Errors

| Status | When | `message` | What the UI should do |
|---|---|---|---|
| 400 | `qrCode` missing or too long | `One or more fields are invalid`, with `errors.QrCode` | Show the field error |
| 400 | QR code invalid or unknown to MoDEE | Text from MoDEE (**English only**), or `The QR code is invalid` when MoDEE gives none | Show "QR code is invalid" |
| 401 | Token missing or expired | `Unauthorized` | Refresh once, as usual |
| 403 | The user doesn't hold `eseal.view` | `Access is forbidden` | Hide the feature (it shouldn't have been shown) |
| 429 | Too many requests | `Too many requests. Try again later` | Wait `Retry-After` seconds |
| 502 | MoDEE unavailable, timed out, or the Sanad settings are wrong | `The e-seal service is unavailable. Try again later` | Show the message with a manual "Try again" button |

- MoDEE's own `400` messages are **not translated**. For a fully Arabic UI, show your own text
  for a `400` without `errors` instead of `message`.

---

## 3. Scans in the audit log

Every call is written to the audit log (`GET /audit-logs`), whether it succeeds or fails. Admins
see the scans of their own branch; the SuperAdmin sees all of them.

| `eventType` | When | `details` |
|---|---|---|
| `qrScanSucceeded` | The sealed document was returned | `QR: <qr code>` |
| `qrScanFailed` | Any error from section 2 | `<status> <message> \| QR: <qr code>`, e.g. `502 The e-seal service is unavailable. Try again later \| QR: ...` |

- `userId` and `userName` are the user who scanned, so the screen can show who did it. `branchId` is their branch. `actorId` is `null`, since the user acted on their own behalf.
- To list one user's scans: `?userId=<id>&eventType=qrScanSucceeded`.
- Validation errors (`400` with `errors`), `401` and the `eseal.view` `403` are rejected before the scan runs, so they are not logged.
- Filter with `?eventType=qrScanSucceeded` or `?eventType=qrScanFailed`.

For the audit log screen, add both events to:
- `src/features/audit/types.ts`: a new group, e.g. `scans: ['qrScanSucceeded', 'qrScanFailed']`
- `src/features/audit/eventMeta.ts`: an icon and tone (`qrScanFailed` as `danger`)
- `src/lib/i18n/locales/en.ts` and `ar.ts`:

| Event | English | Arabic |
|---|---|---|
| `qrScanSucceeded` | QR scanned | مسح رمز QR |
| `qrScanFailed` | QR scan failed | فشل مسح رمز QR |

---

## 4. Using it in the app

### Types and API

```ts
// src/features/eseal/types.ts
export interface SealedDoc {
  signedFile: string; // base64
  contentType: string;
}

// src/features/eseal/api.ts
import { useMutation } from '@tanstack/react-query';
import { api } from '@/lib/api/client';
import type { SealedDoc } from './types';

export const esealApi = {
  sealedDoc: async (qrCode: string) => (await api.post<SealedDoc>('/qr-sanad/sealed-doc', { qrCode })).data,
};

/** A mutation, not a query: each call goes to MoDEE and is audited, so it must never refetch by itself. */
export function useSealedDoc() {
  return useMutation({ mutationFn: esealApi.sealedDoc, retry: false });
}
```

### Viewing and downloading the PDF

```ts
export function toBlob({ signedFile, contentType }: SealedDoc): Blob {
  const binary = atob(signedFile);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
  return new Blob([bytes], { type: contentType || 'application/pdf' });
}

export function openSealedDoc(doc: SealedDoc): void {
  const url = URL.createObjectURL(toBlob(doc));
  window.open(url, '_blank', 'noopener');
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

export function downloadSealedDoc(doc: SealedDoc, fileName = 'sealed-document.pdf'): void {
  const url = URL.createObjectURL(toBlob(doc));
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}
```

To show the PDF inside the page, put the object URL in an `<iframe>` and revoke it when the
component unmounts.

### Typical flow

1. The user scans or pastes the QR code.
2. `sealedDoc.mutate(qrCode)`. Show a loader while `isPending`.
3. On success, show the document and keep `sealedDoc.data` for the "Download" button. Don't call the API again for it.
4. On error, show the message from the table above. Only `502` gets a "Try again" button.
