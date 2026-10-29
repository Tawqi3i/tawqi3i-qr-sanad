# Devices

The plain `user` type is now `device`: a branch's scanning station. Types are `superAdmin`, `admin` and `device`.

- Only a **device** can scan (`POST /qr-sanad/sealed-doc`). The SuperAdmin and admins get `403`, so hide the scan screen for them.
- **Users** (`/users`) are now the SuperAdmin's alone and list the admins (plus the SuperAdmin). An admin gets `403` there.
- **Devices** (`/devices`) are a new screen for the SuperAdmin (every branch) and admins (their own branch).
- The database was rebuilt: every account, branch and scan is gone. The SuperAdmin is created again at first startup.

Route by `type` from `GET /auth/me`: a `device` goes to the scanner; `admin` / `superAdmin` go to the dashboard.

## Devices screen

Same response shapes as `/users` (`UserViewModel` / `UserDetailsViewModel`). For devices, `email` and `phoneNumber` are always `null` and `permissions` is `[]`.

| Endpoint | Permission | Body |
|---|---|---|
| `GET /devices?search=&status=&branchId=&pageNumber=&pageSize=` | `devices.view` | |
| `GET /devices/{id}` | `devices.view` | |
| `POST /devices` | `devices.create` | `{ "userName", "displayName", "password", "branchId" }` |
| `PUT /devices/{id}` | `devices.update` | `{ "displayName" }` |
| `PUT /devices/{id}/status` | `devices.update` | `{ "status": "active" \| "disabled" }` |
| `PUT /devices/{id}/unlock` | `devices.update` | |
| `PUT /devices/{id}/password` | `devices.reset_password` | `{ "newPassword", "mustChangePassword" }` |
| `DELETE /devices/{id}` | `devices.delete` | |

- `branchId` on create: required for the SuperAdmin (show a branch picker). Admins leave it out; the device goes to their branch.
- The device signs in with `userName` and `password` like anyone else. Signing in on a second machine ends the first session.

| Status | `message` |
|---|---|
| 400 | `A branch is required for this type of user` (SuperAdmin without `branchId`) |
| 400 | `The branch is disabled` |
| 404 | `Device not found`, `Branch not found` |
| 409 | `Username is already in use` |
| 409 | `The record was changed by another request. Reload it and try again` (two adds took the last license place at once; retrying shows the limit) |
| 403 | `The license device limit has been reached`, `The license has expired. Contact your provider to renew it` |

## Changes to existing screens

- **Users (SuperAdmin):** `POST /users` creates an admin only. `type` is removed from the body, `branchId` is now required. The `type` filter is removed from `GET /users`. Remove the old "add user" flow for admins.
- **Permissions:** the catalogue is now `devices.view`, `devices.create`, `devices.update`, `devices.delete`, `devices.reset_password`, `audit_logs.view`. `users.*`, `permissions.view` and `eseal.view` no longer exist. `GET /permissions` is SuperAdmin only.
- **Branches:** `usersCount` is renamed to `devicesCount`.
- **Scans:** `GET /qr-sanad/scans` no longer needs a permission. A device sees its own scans; an admin with `audit_logs.view` sees the branch's.
- **License:** `GET /license` adds `maxDevices` (`null` = unlimited) and `devicesCount`. `maxUsers` / `usersCount` now count admins only. Add a usage bar for devices.
- **Profile:** `email` in `GET /auth/me` is `null` for a device.

## Locales

| Key | English | Arabic |
|---|---|---|
| type `device` | Device | جهاز |
| `devices.view` | View the devices of the branch | عرض أجهزة الفرع |
| `devices.create` | Create devices in the branch | إضافة أجهزة إلى الفرع |
| `devices.update` | Update devices, change their status and unlock them | تعديل الأجهزة وتغيير حالتها وفك قفلها |
| `devices.delete` | Delete devices | حذف الأجهزة |
| `devices.reset_password` | Reset a device's password | إعادة تعيين كلمة مرور الجهاز |
