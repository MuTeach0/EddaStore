# Production Deployment Guide — EddaStore

This guide is based on the verified state of the repository on 2026-10-23. It does not contain production secrets and must not be used to connect to a production database without the owner's explicit approval.

## 1. Verified pre-deployment state

| Check | Result |
|---|---|
| `dotnet build -c Release` | Passed: 0 warnings, 0 errors |
| `dotnet test` (Debug) | Passed: 25/25 |
| `dotnet test -c Release` | Passed: 25/25 |
| `dotnet ef migrations has-pending-model-changes` | No pending model changes |
| `dotnet publish -c Release` | Succeeded |
| Vulnerable package scan | No vulnerable packages reported in prior audit; rerun immediately before release |
| Secret scan | No production secrets in source; SMTP/OAuth values are placeholders or user-secrets |

Test breakdown:
- 21 unit/service/security tests.
- 4 SQL Server integration tests (cart soft-deleted lines, checkout idempotency, concurrent checkout, customer order isolation). The integration tests use a temporary database and delete it afterward. They run automatically when local SQL Server is available and otherwise pass without executing.

## 2. Production prerequisites

- .NET 10 runtime/SDK on the host or container.
- SQL Server 2019+ or Azure SQL.
- SMTP account for account and order emails.
- Optional Google OAuth client ID/secret.
- HTTPS certificate and reverse proxy (IIS, nginx, Traefik, or cloud load balancer).
- Backup storage and restore procedure.

## 3. Configuration

Configure secrets through environment variables or the ASP.NET Core Secret Manager. Do not commit production values.

Required settings:

```text
ConnectionStrings__DefaultConnection=Server=<host>;Database=<db>;User Id=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True
EmailSettings__Host=<smtp-host>
EmailSettings__Port=587
EmailSettings__Email=<sender>
EmailSettings__Username=<smtp-username>
EmailSettings__AppPassword=<smtp-password>
EmailSettings__DisplayName=EddaStore
```

Optional Google OAuth:

```text
Authentication__Google__ClientId=<client-id>
Authentication__Google__ClientSecret=<client-secret>
```

Notes:
- `Program.cs` throws at startup if `ConnectionStrings:DefaultConnection` is missing.
- Google sign-in is enabled only when both client ID and secret are present.
- Development-only endpoints (`/dev/reset-link`, `/dev/test-email`) are registered only in Development and restricted to loopback clients.
- `CookieSecurePolicy.SameAsRequest` means cookies become Secure automatically when the request is HTTPS. Configure the reverse proxy to forward HTTPS correctly.

## 4. Database migrations

Migration files:
- `20261022151652_InitialCreate`
- `20261023073906_AddCheckoutIdempotencyAndWishlist`

Reviewed migration behavior:
- Adds `CheckoutToken nvarchar(64) NOT NULL` and `CouponCode nvarchar(50) NULL` to `Orders`.
- Creates `WishlistItems`.
- Backfills empty/null `CheckoutToken` values with `LOWER(CONVERT(nvarchar(36), NEWID()))`.
- Creates unique index `IX_Orders_CustomerId_CheckoutToken` on `(CustomerId, CheckoutToken)`.
- Creates filtered unique wishlist index on `(CustomerId, ProductId)` where `IsDeleted = 0`.
- Migration order and `ApplicationDbContextModelSnapshot` are consistent.
- Applying the migration to an existing database updates schema without dropping tables or data.
```

Important idempotency review:
- The unique index is scoped to `(CustomerId, CheckoutToken)`, so two different customers may use the same checkout token, while the same customer cannot create duplicate orders for the same token.
- Legacy empty tokens are backfilled before index creation. The backfill uses `NEWID()`, so existing rows receive distinct values and do not violate the new unique index.
- Checkout code performs an initial read and a second read inside a serializable transaction. Concurrent duplicate submissions are covered by the database unique index and retry logic.

Migration script review:
- Full script: `artifacts/migrations.sql`.
- Delta script for an already-initialized database: `artifacts/migrations-delta.sql`.
- Regenerate before release:

```powershell
dotnet ef migrations script 20261022151652_InitialCreate 20261023073906_AddCheckoutIdempotencyAndWishlist --output artifacts/migrations-delta.sql
```

Review the delta script for:
- No `DROP TABLE` or `DROP COLUMN` statements.
- `ALTER TABLE ... ADD` only for new columns.
- Backfill `UPDATE` before unique index creation.
- Unique index names and filters.
- `INSERT INTO [__EFMigrationsHistory]` for the new migration.

Apply to production using one of these methods:

```powershell
# Option A: EF Core CLI (requires production network access and credentials)
dotnet ef database update --connection "<production-connection-string>"

# Option B: SQL script (recommended for production review/approval)
sqlcmd -S <server> -d <database> -i artifacts/migrations-delta.sql
```

Do not run `dotnet ef database update` against production until the script has been reviewed and a backup exists.

Rollback:
- Keep a database backup before migration.
- The migration `Down` method drops `WishlistItems`, `CheckoutToken`, and `CouponCode`. Rolling back loses wishlist data and checkout-token idempotency data.
- Prefer restore-from-backup over `Down` for production incidents.

## 5. Build and publish

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet ef migrations has-pending-model-changes
dotnet publish -c Release -o .\artifacts\publish
```

Expected:
- Build: 0 warnings, 0 errors.
- Tests: 25/25 passed when SQL Server is available.
- No pending model changes.
- Publish output created successfully.

## 6. HTTPS, HSTS, and security headers

- `app.UseHsts()` is enabled outside Development.
- `app.UseHttpsRedirection()` is enabled.
- Security headers are added:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- Authentication cookies use `HttpOnly`, `SameSite=Lax`, sliding expiration, and `SecurePolicy.SameAsRequest`.
- Rate limiting is applied with `[EnableRateLimiting("auth")]` to Login, Register, and ForgotPassword. The current limiter allows 5 requests per minute per remote IP address.
- Admin controllers are protected by `[Authorize(Roles = "Admin")]`.
- `AccountController.AddAddress` uses an allow-list `[Bind(...)]`.

Recommended before launch:
- Terminate TLS at the reverse proxy and forward `X-Forwarded-Proto`.
- Verify HSTS header in production.
- Consider adding Content-Security-Policy after testing inline assets.

## 7. Health check

- Endpoint: `/health`
- Implementation: `AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()`
- The endpoint verifies database connectivity.
- Default response is a minimal status (`Healthy`) and does not expose connection strings, stack traces, or other sensitive details.
- Use it for load balancer and container health probes.

## 8. Admin seeding

The application creates the `Admin` role at startup if it does not exist. Create the first admin user manually in production using a secure process. Do not store admin passwords in source control.

## 9. Backups and restore

- Enable automated SQL Server backups (full + differential/log as appropriate).
- Test restore to a non-production server before launch.
- Document RPO/RTO and owner contacts.
- Keep at least one pre-migration backup.
- Verify cleanup commands are never run against production.

## 10. Post-deployment smoke tests

- `/health` returns 200.
- Catalog browsing, filtering, sorting, pagination.
- Product details and reviews.
- Register, login, logout, forgot password.
- Add/update/remove cart items.
- Checkout with a valid address.
- Re-submitting the same checkout token returns the same order.
- Stock decreases once.
- Order appears in customer history.
- Admin login and order status update.
- Cancelled order restores stock and cancels payment.
- Order confirmation email.

## 11. Remaining risks and owner decisions

1. Production SQL Server connection string and migration execution.
2. Real SMTP credentials.
3. HTTPS certificate and reverse proxy configuration.
4. Payment method decision: cash on delivery only or payment gateway integration.
5. Production load, security, and backup/restore testing.
6. Admin MFA decision.
7. Content-Security-Policy rollout after testing inline assets.
8. Rate limiter threshold is 5 requests/minute per IP for auth endpoints; adjust based on expected traffic.
9. No online payment gateway is implemented.
10. No production monitoring/alerting is configured yet.

## 12. Useful commands

```powershell
# Build
dotnet build -c Release

# Test
dotnet test -c Release

# Pending model changes
dotnet ef migrations has-pending-model-changes

# Publish
dotnet publish -c Release -o .\artifacts\publish

# Migration script
dotnet ef migrations script 20261022151652_InitialCreate 20261023073906_AddCheckoutIdempotencyAndWishlist --output artifacts/migrations-delta.sql

# User secrets (development only)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>"
```
