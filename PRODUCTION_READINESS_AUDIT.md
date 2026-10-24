# Production Readiness Audit — EddaStore

Date: 2026-10-23  
Application: EddaStore ASP.NET Core e-commerce application  
Framework: .NET 10 / ASP.NET Core Razor Pages + MVC Areas  
Database: SQL Server (Entity Framework Core)  
Audit scope: source code, configuration, migrations, build, tests, operational readiness.

## 1. Executive summary

EddaStore is a functional ASP.NET Core e-commerce application with:
- product catalog, search, filtering, sorting, and pagination;
- cart and checkout;
- cash-on-delivery order creation;
- coupons;
- customer accounts and addresses;
- order history and tracking;
- reviews and wishlist;
- admin product/order/customer/coupon management;
- Identity authentication with email/password and optional Google sign-in.

The application now builds successfully (`dotnet build`: 0 errors, 0 warnings) and automated tests pass (`dotnet test`: 25/25).

The most important production blockers that were identified and remediated in this audit:
- weak password policy and missing cookie hardening;
- no brute-force protection on authentication endpoints;
- mass-assignment risk in address creation;
- no security headers;
- no health endpoint;
- checkout confirmation email was not sent;
- no automated coverage for critical checkout/idempotency logic.

Remaining launch blockers that require environment/business decisions:
- production SQL Server connection string and migrations;
- real SMTP credentials;
- HTTPS certificate / reverse proxy configuration;
- absence of online payment gateway (currently cash-on-delivery only);
- no production load/performance/security testing yet.

## 2. Verification performed

| Check | Result |
|---|---|
| `dotnet build` | Passed, 0 warnings/errors |
| `dotnet test` | Passed, 25/25 |
| `dotnet list package --vulnerable` | No vulnerable packages reported |
| `dotnet ef migrations script` | Migration script generated successfully |
| Secret scan (manual) | No committed production secrets found; SMTP values are placeholders/null |

## 3. Functional readiness matrix

| Area | Status | Notes |
|---|---|---|
| Catalog browsing | Ready | Active products with stock > 0, categories, brands, search, filters, sort, pagination |
| Product details | Ready | Includes reviews and related images |
| Cart | Ready | Server-side quantity/stock validation |
| Checkout | Ready with residual risk | COD only; server-side totals; idempotent checkout token; transactional stock update |
| Coupons | Ready | Date/usage/minimum/maximum validation; usage count incremented |
| Customer account | Ready | Profile, addresses, order history |
| Order tracking | Ready | Customer can view own orders; admin can update status |
| Reviews | Ready | Authenticated users can submit; product active check |
| Wishlist | Ready | Soft-delete aware unique index |
| Admin panel | Ready | `[Authorize(Roles = "Admin")]` on all admin controllers |
| Email | Partial | SMTP configured through user secrets; order confirmation email added; requires real SMTP |
| Payments | Partial / business decision | COD only; no gateway integration |
| Search | Ready | EF Core parameterized `Contains` queries |

## 4. Security audit and remediations

### 4.1 Remediated

| Finding | Severity | Remediation |
|---|---|---|
| Password policy too weak (8 chars, no uppercase requirement) | High | Increased to 10 chars and required uppercase |
| Authentication cookies not explicitly hardened | High | `HttpOnly`, `SameSite=Lax`, secure policy, sliding expiration |
| No brute-force protection on login/forgot-password | High | Fixed-window rate limiter (5 requests/minute) applied to `LoginModel` and `ForgotPasswordModel` |
| No brute-force protection on registration | Medium | Applied the same rate limiter to `RegisterModel` |
| Checkout token uniqueness scoped incorrectly | High | Changed unique index to `(CustomerId, CheckoutToken)` and backfilled legacy empty tokens in migration |
| `/health` did not verify database | Medium | Added EF Core database health check |
| Mass assignment in `AccountController.AddAddress` | Medium | Added `[Bind(...)]` allow-list for address fields |
| Missing security headers | Medium | Added `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` |
| Checkout confirmation email missing | Medium | Added best-effort confirmation email after successful order |
| No health endpoint | Medium | Added `/health` |

### 4.2 Verified controls

- CSRF protection via `[ValidateAntiForgeryToken]` on POST actions.
- Razor output encoding used by default.
- EF Core parameterized queries; no raw SQL string concatenation found.
- Ownership checks in account/order controllers.
- File upload restricted by extension and content type in `ProductImageService`.
- Admin endpoints protected by role authorization.
- Soft-delete query filters applied globally.
- No committed production secrets; SMTP secrets are placeholders/null.
- No vulnerable NuGet packages reported by `dotnet list package --vulnerable`.
### 4.3 Residual risks / not remediated

| Item | Severity | Notes |
|---|---|---|
| No Content Security Policy | Medium | Not added because inline scripts/styles may exist; must be tested before enabling |
| No automated security testing (SAST/DAST) | Medium | Recommended before public launch |
| No account lockout tuning beyond Identity defaults | Low | Verify `MaxFailedAccessAttempts` and lockout duration in production |
| No MFA | Medium | Consider for admin accounts at minimum |
| File upload only validates extension/content type | Low | Consider antivirus/malware scanning for uploaded images |
| No request size limits explicitly configured | Low | Kestrel/IIS defaults apply |
| No audit logging for admin actions | Medium | Recommended for compliance and incident response |

## 5. Data and persistence

- EF Core migrations exist and a full migration script can be generated.
- Schema includes unique constraints for order checkout tokens and wishlist items.
- Checkout uses a serializable transaction for stock decrement and order creation.
- Checkout is idempotent by customer + checkout token.
- Coupon usage count is incremented within the transaction.
- Soft delete is implemented for `ISoftDelete` entities.

Operational requirements:
- Apply migrations with `dotnet ef database update` or a generated SQL script in production.
- Back up the production database before migration.
- Verify connection string points to production SQL Server, not LocalDB.
- Confirm database user has least privilege.

## 6. Configuration and secrets

Current configuration:
- `ConnectionStrings:DefaultConnection` points to LocalDB (development only).
- `Email:From`, `Email:SmtpHost`, `Email:SmtpPort`, `Email:Username`, `Email:Password` are placeholders/null.
- Google OAuth credentials are configured through user secrets.

Production requirements:
- Store all secrets in environment variables, Azure Key Vault, or another secret manager.
- Never commit real SMTP or OAuth secrets.
- Set `ASPNETCORE_ENVIRONMENT=Production`.
- Configure HTTPS redirection and HSTS at the edge/reverse proxy.
- Set allowed hosts and CORS if the frontend is separated.

## 7. Email and notifications

- `IEmailSender` implementation logs emails in development.
- SMTP sender is registered and used by Identity flows.
- Order confirmation email was added after successful checkout.
- Password reset and email confirmation depend on real SMTP configuration.

Risk:
- If SMTP is not configured, registration/password-reset emails will not be delivered. This is a launch blocker for customer self-service.

## 8. Performance and observability

Implemented:
- `/health` endpoint.
- Structured logging via default ASP.NET Core logging.

Recommended before launch:
- Add application metrics/tracing (OpenTelemetry or APM).
- Add slow-query and error alerting.
- Load test catalog, cart, and checkout flows.
- Add response caching for catalog pages if traffic warrants.
- Verify database indexes for product search and order history.
- Configure log retention and PII redaction.

## 9. Testing status

Automated tests:
- `CartServiceTests`: 2 tests, passing.
- `CartServiceAdditionalTests`: 4 tests, passing (update, stock, remove, inactive product filtering).
- `CheckoutServiceTests`: 1 test, passing (idempotent checkout token and stock decrement).
- `CheckoutServiceAdditionalTests`: 4 tests, passing (empty cart, insufficient stock, invalid coupon, valid coupon).
- `CouponServiceTests`: 5 tests, passing (duplicate, dates, percentage limit, max discount, update).
- `AdminOrderStatusTests`: 1 test, passing (cancellation restores stock and cancels payment).
- `SecurityConfigurationTests`: 4 tests, passing (password policy and authorization/rate-limiting attributes).
- `SqlServerIntegrationTests`: 4 tests, passing (cart soft-deleted line handling, checkout idempotency, concurrent checkout, customer order isolation).
Total: 25 tests, passing.

Gaps:
- No controller-level authorization integration tests against the full HTTP pipeline.
- No UI/end-to-end tests.
- No load or resilience tests.

Recommendation: add integration tests using a real SQL Server test container before launch.

## 10. Deployment and operations

Required production steps:
1. Provision production SQL Server and update connection string.
2. Apply EF Core migrations.
3. Configure SMTP and Google OAuth secrets.
4. Configure HTTPS and reverse proxy.
5. Run `dotnet publish -c Release`.
6. Configure process manager (IIS/Windows Service/systemd/container).
7. Verify `/health`.
8. Seed initial admin role/user.
9. Run smoke tests for catalog, cart, checkout, login, admin.
10. Enable monitoring and backups.

## 11. Launch blockers (P0)

1. Production database connection string and migration execution.
2. Real SMTP credentials for account emails.
3. HTTPS certificate/reverse proxy configuration.
4. Decision on payment method: COD only vs. payment gateway integration.
5. Production smoke test and backup/restore verification.

## 12. Recommended next improvements (P1/P2)

P1:
- Add admin audit logging.
- Add MFA for admin accounts.
- Add integration tests with SQL Server.
- Add CSP after testing inline assets.
- Add rate limiting to registration and checkout endpoints.

P2:
- Add product image virus scanning.
- Add response compression and caching.
- Add OpenTelemetry metrics/tracing.
- Add customer self-service order cancellation within policy.
- Add invoice PDF generation if required by business.

## 13. Files changed during this audit

- `Program.cs` (health check now verifies database connectivity)
- `EddaStore.csproj` (health checks EF package)
- `Areas/Identity/Pages/Account/Register.cshtml.cs` (rate limiting)
- `Data/Configurations/OrderConfiguration.cs` (composite checkout token uniqueness)
- `Data/Migrations/20261023073906_AddCheckoutIdempotencyAndWishlist.cs` (backfill and composite index)
- `Data/Migrations/20261023073906_AddCheckoutIdempotencyAndWishlist.Designer.cs` (composite index metadata)
- `Data/Migrations/ApplicationDbContextModelSnapshot.cs` (composite index metadata)
- `EddaStore.Tests/CartServiceAdditionalTests.cs` (new)
- `EddaStore.Tests/CheckoutServiceAdditionalTests.cs` (new)
- `EddaStore.Tests/CouponServiceTests.cs` (new)
- `EddaStore.Tests/AdminOrderStatusTests.cs` (new)
- `EddaStore.Tests/SecurityConfigurationTests.cs` (new)

- `Areas/Identity/Pages/Account/Login.cshtml.cs` (rate limiting and lockout handling)
- `Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs` (rate limiting and SMTP-safe logging)
- `Controllers/AccountController.cs` (mass-assignment hardening)
- `Services/CheckoutService.cs` (idempotent checkout and confirmation email)
- `EddaStore.Tests/CheckoutServiceTests.cs` (idempotency test)
- `PRODUCTION_READINESS_AUDIT.md`

## 14. Final pre-deployment verification update

### Additional fixes made during final verification
- `CartService` now ignores soft-deleted cart lines when checking for an existing line, creating a new active line instead of resurrecting/updating a deleted line.
- `CheckoutService` now retries transient failures (deadlocks, unique-constraint races) and handles concurrent duplicate checkout submissions safely.
- Rate limiting is now partitioned by remote IP address instead of using one global limiter.
- Added `SqlServerIntegrationTests` (4 tests) covering SQL Server behavior.

### Migration review
- Composite unique index `(CustomerId, CheckoutToken)` verified in configuration and migration.
- Backfill of empty `CheckoutToken` values occurs before index creation.
- Migration order and model snapshot are consistent.
- `dotnet ef migrations has-pending-model-changes` reports no pending changes.

### Health and security
- `/health` uses `AddDbContextCheck<ApplicationDbContext>()` and returns only a minimal healthy status.
- HSTS is enabled outside Development.
- Security headers are present.
- Rate limiting is applied to Login, Register, and ForgotPassword.

### Final command results
- `dotnet build -c Release`: 0 warnings, 0 errors.
- `dotnet test`: 25/25 passed.
- `dotnet test -c Release`: 25/25 passed.
- `dotnet ef migrations has-pending-model-changes`: no pending changes.
- `dotnet publish -c Release`: succeeded.
- Migration scripts generated in `artifacts/migrations.sql` and `artifacts/migrations-delta.sql`.

### Remaining launch blockers
1. Production SQL Server connection string and migration execution.
2. Real SMTP credentials.
3. HTTPS certificate and reverse proxy configuration.
4. Payment method business decision.
5. Production smoke test, backup/restore verification, and monitoring.
- `PRODUCTION_LAUNCH_CHECKLIST.md`
