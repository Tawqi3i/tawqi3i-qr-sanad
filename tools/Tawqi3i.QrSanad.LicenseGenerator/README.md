# QrSanad license generator

Issues the license keys the QrSanad API requires. Only whoever holds the private key can issue
licenses. The API embeds the matching public key in
`src/Infrastructure/Tawqi3i.QrSanad.Licensing/LicensePublicKey.cs`.

The private key must never go into source control. If it leaks, anyone can issue licenses. If it
is lost, no new licenses can be issued until a new pair is generated, the public key is replaced
and every customer gets a new license.

## Issue a license

Quickest: edit the values at the top of `Program.cs` (customer, type, expiry, limits and the
private key path) and Run (F5). The key is printed in the console.

From the command line instead:

```bash
dotnet run --project tools/Tawqi3i.QrSanad.LicenseGenerator -- issue \
  --private-key <path-to-private.pem> \
  --customer "Customer name" --type PROD \
  --days 365 --max-users 50 --max-branches 5
```

| Option | Notes |
|---|---|
| `--type` | `TRIAL`, `PROD` or `DEV` |
| `--days` or `--expires yyyy-MM-dd` | Expiry, UTC |
| `--max-users` | Admins and users, not counting the SuperAdmin. Omit for unlimited |
| `--max-branches` | Omit for unlimited |

The key is printed on one line. It goes either in `Licensing:Key` (appsettings or the
`Licensing__Key` environment variable), or to the SuperAdmin to install with `PUT /api/v1/license`.
The tool warns if the private key does not match the public key built into the API.

## Check a license

```bash
dotnet run --project tools/Tawqi3i.QrSanad.LicenseGenerator -- verify <license-key>
```

## New key pair

```bash
dotnet run --project tools/Tawqi3i.QrSanad.LicenseGenerator -- keygen <directory>
```

This writes `private.pem` and prints the public key for `LicensePublicKey.Value`. Every license
issued with the old pair stops working once the API is built with the new public key.

## Format

`QRS-LIC-{payload}.{signature}`: the JSON license and its ECDSA P-256 / SHA-256 signature, both
base64url. The payload can be read by anyone; the signature is what stops it from being changed.
