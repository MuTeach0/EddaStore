using System.Data;
using EddaStore.Constants;
using EddaStore.Data;
using EddaStore.Models.Orders;
using EddaStore.Models.Payments;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace EddaStore.Services;

public class CheckoutService(ApplicationDbContext context, IEmailSender emailSender, ILogger<CheckoutService> logger) : ICheckoutService
{
    public async Task<Guid> CreateOrderAsync(Guid customerId, CheckoutInputViewModel input, CancellationToken ct = default)
    {
        const int maxAttempts = 3;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0)
            {
                context.ChangeTracker.Clear();
                await Task.Delay(25 * (attempt + 1), ct);
            }

            try
            {
                return await CreateOrderCoreAsync(customerId, input, ct);
            }
            catch (Exception ex) when (attempt < maxAttempts - 1 && (IsTransient(ex) || IsUniqueConstraintViolation(ex)))
            {
                continue;
            }
        }

        throw new InvalidOperationException("Unable to complete checkout.");
    }


    private async Task<Guid> CreateOrderCoreAsync(Guid customerId, CheckoutInputViewModel input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.CheckoutToken)) throw new ArgumentException("رمز إتمام الطلب غير صالح.");
        var prior = await context.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.CustomerId == customerId && x.CheckoutToken == input.CheckoutToken, ct);
        if (prior is not null) return prior.Id;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            prior = await context.Orders.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.CheckoutToken == input.CheckoutToken, ct);
            if (prior is not null) { await transaction.CommitAsync(ct); return prior.Id; }
            var cart = await context.Carts.Include(x => x.Items).FirstOrDefaultAsync(x => x.CustomerId == customerId, ct)
                ?? throw new InvalidOperationException("سلة التسوق فارغة.");
            if (cart.Items.Count == 0) throw new InvalidOperationException("سلة التسوق فارغة.");
            var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToArray();
            var products = await context.Products.Where(x => productIds.Contains(x.Id)).OrderBy(x => x.Id).ToDictionaryAsync(x => x.Id, ct);
            if (products.Count != productIds.Length) throw new InvalidOperationException("أحد المنتجات لم يعد متاحًا.");
            foreach (var line in cart.Items)
                if (!products[line.ProductId].IsActive || products[line.ProductId].StockQuantity < line.Quantity)
                    throw new InvalidOperationException($"الكمية المطلوبة من «{products[line.ProductId].Name}» لم تعد متاحة.");

            var subtotal = cart.Items.Sum(x => products[x.ProductId].Price * x.Quantity);
            decimal discount = 0; string? couponCode = null;
            if (!string.IsNullOrWhiteSpace(input.CouponCode))
            {
                couponCode = input.CouponCode.Trim().ToUpperInvariant();
                var coupon = await context.Coupons.FirstOrDefaultAsync(x => x.Code == couponCode, ct);
                var now = DateTimeOffset.UtcNow;
                if (coupon is null || !coupon.IsActive || coupon.StartsAt > now || coupon.ExpiresAt <= now || (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit) || (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount))
                    throw new InvalidOperationException("كوبون الخصم غير صالح لهذا الطلب.");
                discount = coupon.DiscountType == "Percentage" ? subtotal * coupon.DiscountValue / 100m : coupon.DiscountValue;
                if (coupon.MaximumDiscountAmount.HasValue) discount = Math.Min(discount, coupon.MaximumDiscountAmount.Value);
                discount = Math.Min(discount, subtotal); coupon.UsageCount++;
            }

            var order = new Order { CustomerId = customerId, CheckoutToken = input.CheckoutToken, CouponCode = couponCode, OrderNumber = "ED-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), Status = OrderStatuses.Pending, Subtotal = subtotal, DiscountAmount = discount, ShippingCost = 0m, TotalAmount = subtotal - discount, Currency = "EGP", PlacedAt = DateTimeOffset.UtcNow };
            foreach (var line in cart.Items) { var p = products[line.ProductId]; p.StockQuantity -= line.Quantity; order.Items.Add(new OrderItem { ProductId = p.Id, ProductName = p.Name, SKU = p.SKU, UnitPrice = p.Price, Quantity = line.Quantity, LineTotal = p.Price * line.Quantity }); }
            context.Orders.Add(order);
            context.OrderAddresses.Add(new OrderAddress { Order = order, RecipientName = input.RecipientName.Trim(), PhoneNumber = input.PhoneNumber.Trim(), City = input.City.Trim(), StreetAddress = input.StreetAddress.Trim(), Building = input.Building?.Trim(), Apartment = input.Apartment?.Trim(), PostalCode = input.PostalCode?.Trim() });
            context.Payments.Add(new Payment { Order = order, Provider = "CashOnDelivery", Status = "Pending", Amount = order.TotalAmount });
            context.OrderStatusHistories.Add(new OrderStatusHistory { Order = order, Status = OrderStatuses.Pending, ChangedAt = DateTimeOffset.UtcNow });
            context.CartItems.RemoveRange(cart.Items);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            try
            {
                var customer = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customerId, ct);
                if (!string.IsNullOrWhiteSpace(customer?.Email))
                {
                    var itemsHtml = string.Join("", order.Items.Select(item => $"<li>{System.Net.WebUtility.HtmlEncode(item.ProductName)} × {item.Quantity} — {item.LineTotal:N0} ج.م</li>"));
                    await emailSender.SendEmailAsync(
                        customer.Email!,
                        "تأكيد الطلب - عِدّة",
                        $"<p>تم استلام طلبك رقم <strong>{order.OrderNumber}</strong>.</p><ul>{itemsHtml}</ul><p>الإجمالي: {order.TotalAmount:N0} ج.م</p><p>طريقة الدفع: الدفع عند الاستلام.</p>");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Order {OrderId} created but confirmation email could not be sent.", order.Id);
            }

            return order.Id;
        }
        catch { await transaction.RollbackAsync(ct); throw; }
    }
    private static bool IsTransient(Exception exception)
    {
        if (exception is SqlException sqlException)
        {
            return sqlException.Number is 1205 or -2 or 4060 or 40197 or 40501 or 49918 or 49919 or 49920;
        }

        if (exception is DbUpdateException dbUpdateException && dbUpdateException.InnerException is not null)
        {
            return IsTransient(dbUpdateException.InnerException);
        }

        return exception.InnerException is not null && IsTransient(exception.InnerException);
    }

    private static bool IsUniqueConstraintViolation(Exception exception)
    {
        if (exception is DbUpdateException dbUpdateException)
        {
            if (dbUpdateException.InnerException is SqlException sqlException)
            {
                return sqlException.Number is 2601 or 2627;
            }
        }

        if (exception.InnerException is not null)
        {
            return IsUniqueConstraintViolation(exception.InnerException);
        }

        return false;
    }

}
