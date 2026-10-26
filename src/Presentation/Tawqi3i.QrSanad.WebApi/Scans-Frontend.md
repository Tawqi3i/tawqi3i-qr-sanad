# QR scans list: frontend changes

New endpoint that lists the QR scans, with search by national number or name.

## `GET /qr-sanad/scans`

Requires `eseal.view`. What the caller sees depends on their permissions:

| Caller | Sees |
|---|---|
| SuperAdmin | Every scan |
| Holds `audit_logs.view` | The scans of their own branch |
| Only `eseal.view` | Only their own scans |

### Query

| Parameter | Notes |
|---|---|
| `search` | Optional, up to 256 characters. Digits match the national number **from its start** (`2000` finds `2000000001`). Anything else matches part of the Arabic or English name; English is case-insensitive |
| `userId` | Optional, the scans of one user |
| `from`, `to` | Optional, UTC. `from` inclusive, `to` exclusive |
| `pageNumber`, `pageSize` | As in every list (default 1 and 20, `pageSize` at most 100) |

A search only finds successful scans of identity cards, because only those have a holder.

### `200`

Paged, newest first. Each item has the same shape as an audit log entry:

```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1,
  "pageCount": 1,
  "items": [
    {
      "id": "0199...",
      "eventType": "qrScanSucceeded",
      "userId": "0199...",
      "userName": "ahmad",
      "branchId": "0199...",
      "details": "QR: ...",
      "identityCard": {
        "nationalNumber": "2000000001",
        "arabicName": "محمد احمد علي",
        "englishName": "MOHAMMAD AHMAD ALI",
        "...": "every field, see IdentityCard-Frontend.md"
      },
      "createdAt": "2026-10-25T12:00:00Z"
    }
  ]
}
```

The same paged shape as `GET /audit-logs`, so the existing audit log types and table can be reused.

- `eventType` is `qrScanSucceeded` or `qrScanFailed`. For a failed scan, `details` holds the error.
- `identityCard` is `null` for failed scans and documents that are not identity cards.

### Errors

| Status | When |
|---|---|
| 400 | `search` longer than 256 characters, or a bad paging value (`errors` holds the field) |
| 403 | The user doesn't hold `eseal.view` |

## Files to change

```ts
// src/features/eseal/api.ts
export interface ScansQuery {
  search?: string;
  userId?: string;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}

scans: async (query: ScansQuery) =>
  (await api.get<Paged<AuditLogEntry>>('/qr-sanad/scans', { params: query })).data,
```

A scans page behind `eseal.view` with one search box (national number or name), a date range,
and columns for time, user, holder name, national number and status. Debounce the search box
before sending it.
