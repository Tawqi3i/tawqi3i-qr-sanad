# QR scan statistics for the dashboard

A new endpoint returns QR scan counts for the dashboard: totals, per day, top users and, for the SuperAdmin, per branch.

## `GET /qr-sanad/stats`

**Requires `audit_logs.view`**, the same as the audit log. Scoped the same way too: the SuperAdmin sees every branch, admins and users only their own.

| Query | Default | Notes |
|---|---|---|
| `from` | Start of the viewer's day, 29 days ago | UTC, inclusive |
| `to` | Now | UTC, exclusive |
| `utcOffsetMinutes` | `0` | The viewer's offset, so days follow their calendar: `-new Date().getTimezoneOffset()` (Jordan: `180`). Range -720 to 840 |

Sending only `utcOffsetMinutes` gives the last 30 days. The range can be at most 92 days.

```
GET /qr-sanad/stats?utcOffsetMinutes=180
```

`200`:

```json
{
  "data": {
    "from": "2026-09-25T21:00:00Z",
    "to": "2026-10-24T09:12:00Z",
    "succeeded": 412,
    "failed": 23,
    "total": 435,
    "daily": [
      { "date": "2026-09-26", "succeeded": 12, "failed": 1, "total": 13 },
      { "date": "2026-09-27", "succeeded": 0, "failed": 0, "total": 0 }
    ],
    "topUsers": [
      { "userId": "0199…", "userName": "user.one", "succeeded": 120, "failed": 4, "total": 124 }
    ],
    "branches": [
      { "branchId": "0199…", "branchName": "Amman Branch", "succeeded": 300, "failed": 10, "total": 310 }
    ]
  }
}
```

- `daily` has one entry per day of the range, oldest first, **including days with no scans**. The last entry is today.
- `topUsers`: the 5 users with the most scans.
- `branches`: SuperAdmin only. It is `null` for everyone else. `branchName` is `null` if the branch was deleted.

| Status | `message` |
|---|---|
| 400 | `The start date must be before the end date, at most 92 days apart` |
| 400 | `One or more fields are invalid`, with `errors.UtcOffsetMinutes` |
| 403 | `Access is forbidden` (no `audit_logs.view`) |

## Suggested dashboard widgets

Show them only when `can(PERMISSIONS.auditLogsView)`.

- Stat cards (existing `StatCard`):
  - Scans, last 30 days: `total`
  - Successful: `succeeded`, tone `success`
  - Failed: `failed`, tone `danger`
  - Today: `daily.at(-1).total`
- A daily chart from `daily`, with succeeded and failed stacked. There is no chart library in the project yet; adding one (e.g. `recharts`) is up to you.
- Top users list from `topUsers`, linking to `/audit-logs?userId=…&eventType=qrScanSucceeded`.
- SuperAdmin: a per-branch table from `branches`.

```ts
// src/features/dashboard/api.ts
export const scanStatsApi = {
  get: () => api.get<ScanStats>('/qr-sanad/stats', { utcOffsetMinutes: -new Date().getTimezoneOffset() }),
};
```

Each call reads up to 92 days of scans, so don't poll it. Load it when the dashboard opens; a `staleTime` of a minute or two is enough.
