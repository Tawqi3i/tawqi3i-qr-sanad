# Identity card holder: frontend changes

When the sealed document of a QR scan is a digital identity card, the API reads the card's fields
(everything except the photo), returns them, and stores them with the scan. Nothing else about the
endpoints changed.

## The `identityCard` object

Used in the three places below.

```json
{
  "nationalNumber": "2000000001",
  "arabicName": "محمد احمد علي",
  "englishName": "MOHAMMAD AHMAD ALI",
  "gender": "ذكر",
  "bloodType": "O+",
  "birthDate": "2000-07-16",
  "motherName": "سلمى",
  "registryNumber": "013/049",
  "expiryDate": "2032-07-20",
  "residence": "حلاوه/قصبه عجلون",
  "cardNumber": "ABC12345"
}
```

| Field | Card label | Notes |
|---|---|---|
| `nationalNumber` | الرقم الوطني | 10 digits. Always set |
| `arabicName` | (top right) | Always set |
| `englishName` | (top left) | Upper case, as printed. Always set |
| `gender` | الجنس | As printed: `ذكر` or `أنثى` |
| `bloodType` | فصيلة الدم | e.g. `O+` |
| `birthDate` | تاريخ الميلاد | `yyyy-MM-dd` |
| `motherName` | اسم الأم | |
| `registryNumber` | رقم القيد | |
| `expiryDate` | تاريخ الإنتهاء | `yyyy-MM-dd` |
| `residence` | مكان الإقامة | |
| `cardNumber` | رقم الهوية | The card's own number, **not** the national number |

Any field other than the first three is `null` when it is missing from the card. The values are
Arabic text as printed, so don't translate them.

They are personal data: don't put them in the URL, local storage or analytics.

## 1. `POST /qr-sanad/sealed-doc`

The response has a new `identityCard`:

```json
{
  "data": {
    "signedFile": "JVBERi0xLjQK...",
    "contentType": "application/pdf",
    "identityCard": { "...": "the object above" }
  }
}
```

`identityCard` is `null` for any other document, or when the card could not be read. The scan
still succeeds and `signedFile` is still there. No new errors.

## 2. `GET /audit-logs`

Each entry has a new `identityCard`: the object above on `qrScanSucceeded` entries of identity
cards, `null` everywhere else (including scans made before this change).

## 3. `GET /qr-sanad/scans`

New list of scans with search by national number or name. See `Scans-Frontend.md`.

## Files to change

```ts
// src/features/eseal/types.ts
export interface IdentityCard {
  nationalNumber: string;
  arabicName: string;
  englishName: string;
  gender: string | null;
  bloodType: string | null;
  birthDate: string | null; // yyyy-MM-dd
  motherName: string | null;
  registryNumber: string | null;
  expiryDate: string | null; // yyyy-MM-dd
  residence: string | null;
  cardNumber: string | null;
}

export interface SealedDoc {
  signedFile: string; // base64
  contentType: string;
  identityCard: IdentityCard | null;
}

// src/features/audit/types.ts: add to the audit log entry
identityCard: IdentityCard | null;
```

On the scan result screen, show the card's fields above the document when `identityCard` is set.
On the audit log screen, show the holder's name and national number on `qrScanSucceeded` rows.
