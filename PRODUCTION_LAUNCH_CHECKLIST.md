# Production Launch Checklist — EddaStore

Use this checklist before public launch. Mark each item complete with owner and date.

## A. Code and build
- [ ] `dotnet build -c Release` passes with 0 warnings/errors.
- [ ] `dotnet test` passes.
- [ ] No secrets committed in source control.
- [ ] Release notes/changelog prepared.

## B. Database
- [ ] Production SQL Server provisioned.
- [ ] Connection string configured via secret manager.
- [ ] Database backup strategy configured and tested.
- [ ] Migrations applied (`dotnet ef database update` or SQL script).
- [ ] Migration rollback plan documented.
- [ ] Seed data for categories/brands/products reviewed.
- [ ] Admin user/role seeded.

## C. Security
- [ ] HTTPS enforced at reverse proxy/edge.
- [ ] HSTS enabled in production.
- [ ] SMTP and OAuth secrets stored securely.
- [ ] Rate limiting verified on login/forgot-password.
- [ ] Security headers verified in browser/network tab.
- [ ] Admin MFA decision made and implemented if required.
- [ ] File upload storage permissions reviewed.
- [ ] Dependency vulnerability scan rerun immediately before release.

## D. Email
- [ ] SMTP host/port/credentials configured.
- [ ] Sender address verified.
- [ ] Registration confirmation email tested.
- [ ] Password reset email tested.
- [ ] Order confirmation email tested.

## E. Commerce flows (smoke tests)
- [ ] Browse catalog and filters.
- [ ] View product details.
- [ ] Add to cart and update quantity.
- [ ] Remove from cart.
- [ ] Apply valid coupon.
- [ ] Reject invalid coupon.
- [ ] Checkout with valid address.
- [ ] Re-submit same checkout token returns same order.
- [ ] Stock decremented correctly.
- [ ] Order appears in customer history.
- [ ] Admin can update order status.
- [ ] Cancelled order restores stock.

## F. Admin
- [ ] Admin login works.
- [ ] Product create/edit/delete works.
- [ ] Image upload/remove works.
- [ ] Order management works.
- [ ] Coupon management works.
- [ ] Customer list/search works.

## G. Operations
- [ ] `/health` returns 200.
- [ ] Logging configured and retained.
- [ ] Monitoring/alerting configured.
- [ ] Backup/restore tested.
- [ ] Incident response contact list prepared.
- [ ] Performance baseline captured.

## H. Business decisions
- [ ] Payment method confirmed (COD only or gateway integration).
- [ ] Shipping cost policy confirmed.
- [ ] Return/cancellation policy documented.
- [ ] Tax/VAT handling confirmed if applicable.

## Sign-off

| Role | Name | Date |
|---|---|---|
| Engineering | | |
| Security | | |
| Operations | | |

## I. Final verification results (2026-10-23)

- [x] `dotnet build -c Release` passes with 0 warnings/errors.
- [x] `dotnet test` passes (25/25).
- [x] `dotnet test -c Release` passes (25/25).
- [x] No pending EF Core model changes.
- [x] `dotnet publish -c Release` succeeds.
- [x] Migration script reviewed (`artifacts/migrations-delta.sql`).
- [x] `/health` verifies database connectivity.
- [x] Rate limiting applied to Login/Register/ForgotPassword.
- [x] Admin controllers require Admin role.
- [x] Concurrent checkout and idempotency tests pass.
- [x] Customer order isolation test passes.
- [x] Cancelled order restores stock and cancels payment test passes.

Remaining owner actions:
- Provision production SQL Server and apply reviewed migration script.
- Configure SMTP and OAuth secrets.
- Configure HTTPS/HSTS/reverse proxy.
- Seed first admin user securely.
- Test backup/restore.
- Run post-deployment smoke tests.
| Product/Owner | | |
