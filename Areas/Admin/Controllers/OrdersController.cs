using EddaStore.Constants;
using EddaStore.Data;
using EddaStore.Models.Invoices;
using EddaStore.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EddaStore.Models.Payments;
using EddaStore.Models.Shipping;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? status, string? search, Guid? customerId, CancellationToken ct)
    {
        var query = context.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.OrderNumber.Contains(search));
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        ViewBag.Statuses = Statuses(); ViewBag.Status = status; ViewBag.Search = search; ViewBag.CustomerId = customerId;
        return View(await query.OrderByDescending(x => x.PlacedAt).ToListAsync(ct));
    }
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await context.Orders.AsNoTracking().Include(x => x.Items).Include(x => x.StatusHistory).Include(x => x.ShippingAddress).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        ViewBag.Statuses = Statuses();
        ViewBag.Payments = await context.Payments.AsNoTracking().Where(x => x.OrderId == id).ToListAsync(ct);
        ViewBag.Shipment = await context.Shipments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == id, ct);
        ViewBag.Customer = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.CustomerId, ct);
        ViewBag.Invoice = await context.Invoices.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.OrderId == id, ct);
        return View(order);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status, CancellationToken ct)
    {
        var order = await context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        var payments = await context.Payments.Where(x => x.OrderId == id).ToListAsync(ct);
        if (!CanTransition(order.Status, status)) { TempData["Error"] = "انتقال حالة الطلب غير مسموح."; return RedirectToAction(nameof(Details), new { id }); }
        if (order.Status != status)
        {
            if (status == OrderStatuses.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == item.ProductId, ct);
                    if (product is not null) product.StockQuantity += item.Quantity;
                }
                foreach (var payment in payments) payment.Status = PaymentStatuses.Cancelled;
            }
            if (status == OrderStatuses.Processing && !PaymentSatisfied(payments))
            {
                TempData["Error"] = "لا يمكن تجهيز الطلب قبل تحقق المبلغ المطلوب.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (status == OrderStatuses.Delivered)
            {
                var shipment = await context.Shipments.FirstOrDefaultAsync(x => x.OrderId == id, ct);
                if (shipment is not null)
                {
                    shipment.Status = "Delivered";
                    shipment.DeliveredAt ??= DateTimeOffset.UtcNow;
                }
            }
            context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = id, Status = status, ChangedAt = DateTimeOffset.UtcNow, Note = "تحديث حالة الطلب من قبل المسؤول." });
            order.Status = status;
            await context.SaveChangesAsync(ct);
        }
        TempData["Success"] = "تم تحديث حالة الطلب.";
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmShipping(Guid id, decimal shippingCost, CancellationToken ct)
    {
        if (shippingCost < 0) { TempData["Error"] = "تكلفة الشحن لا يمكن أن تكون سالبة."; return RedirectToAction(nameof(Details), new { id }); }
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        if (order.Status is OrderStatuses.Cancelled or OrderStatuses.Shipped or OrderStatuses.Delivered)
        {
            TempData["Error"] = "لا يمكن تعديل تكلفة الشحن في هذه الحالة.";
            return RedirectToAction(nameof(Details), new { id });
        }
        order.ShippingCost = shippingCost;
        order.TotalAmount = order.Subtotal - order.DiscountAmount + shippingCost;
        var paymentsForShipping = await context.Payments.Where(x => x.OrderId == id).ToListAsync(ct);
        foreach (var existingPayment in paymentsForShipping)
        {
            existingPayment.RequiredAmount = order.TotalAmount;
            if (existingPayment.Status == PaymentStatuses.Paid && existingPayment.Amount < order.TotalAmount)
                existingPayment.Status = PaymentStatuses.PartiallyPaid;
        }
        var existingInvoiceForShipping = await context.Invoices.FirstOrDefaultAsync(x => x.OrderId == id, ct);
        if (existingInvoiceForShipping is not null)
        {
            existingInvoiceForShipping.Subtotal = order.Subtotal;
            existingInvoiceForShipping.DiscountAmount = order.DiscountAmount;
            existingInvoiceForShipping.TotalAmount = order.TotalAmount;
        }
        context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = id, Status = order.Status, ChangedAt = DateTimeOffset.UtcNow, Note = $"تم تأكيد تكلفة الشحن: {shippingCost:N2} {order.Currency}." });
        await context.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ تكلفة الشحن.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(Guid id, decimal requiredAmount, decimal amountReceived, string? provider, string? transactionReference, bool verified, CancellationToken ct)
    {
        if (requiredAmount <= 0) { TempData["Error"] = "يجب تحديد المبلغ المطلوب (أكبر من صفر)."; return RedirectToAction(nameof(Details), new { id }); }
        if (amountReceived < 0) { TempData["Error"] = "المبلغ المستلم لا يمكن أن يكون سالبًا."; return RedirectToAction(nameof(Details), new { id }); }
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        if (order.Status == OrderStatuses.Cancelled) { TempData["Error"] = "لا يمكن تسجيل دفع لطلب ملغي."; return RedirectToAction(nameof(Details), new { id }); }

        var allPayments = await context.Payments.Where(x => x.OrderId == id).ToListAsync(ct);
        var payment = allPayments.FirstOrDefault();
        var isNew = payment is null;
        if (isNew) payment = new Payment { OrderId = id, Currency = order.Currency };
        var effectiveRequired = requiredAmount > 0 ? Math.Max(requiredAmount, order.TotalAmount) : order.TotalAmount;
        payment!.RequiredAmount = effectiveRequired;
        payment.Amount = amountReceived;
        if (!string.IsNullOrWhiteSpace(provider)) payment.Provider = provider.Trim();
        if (!string.IsNullOrWhiteSpace(transactionReference)) payment.TransactionReference = transactionReference.Trim();

        var otherConfirmedTotal = allPayments.Where(p => p != payment && p.Status == PaymentStatuses.Paid).Sum(p => p.Amount);
        var submittedConfirmedTotal = verified && amountReceived > 0 ? amountReceived : 0m;
        var totalConfirmed = otherConfirmedTotal + submittedConfirmedTotal;

        if (verified)
        {
            if (totalConfirmed >= order.TotalAmount) { payment.Status = PaymentStatuses.Paid; payment.PaidAt ??= DateTimeOffset.UtcNow; }
            else if (totalConfirmed > 0) payment.Status = PaymentStatuses.PartiallyPaid;
            else payment.Status = PaymentStatuses.Pending;
        }
        else if (payment.Status != PaymentStatuses.Paid)
        {
            payment.Status = PaymentStatuses.Pending;
        }
        if (isNew) context.Payments.Add(payment);
        if (verified && payment.Status == PaymentStatuses.Paid && totalConfirmed >= order.TotalAmount)
        {
            var existingInvoice = await context.Invoices.FirstOrDefaultAsync(x => x.OrderId == id, ct);
            if (existingInvoice is null)
            {
                var orderItems = await context.OrderItems.AsNoTracking().Where(x => x.OrderId == id).ToListAsync(ct);
                var invoice = new Invoice
                {
                    OrderId = id,
                    InvoiceNumber = $"INV-{order.OrderNumber}",
                    IssuedAt = DateTimeOffset.UtcNow,
                    Status = "Issued",
                    Currency = order.Currency,
                    Subtotal = order.Subtotal,
                    DiscountAmount = order.DiscountAmount,
                    TaxAmount = 0m,
                    TotalAmount = order.TotalAmount
                };
                foreach (var item in orderItems)
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        ProductName = item.ProductName,
                        SKU = item.SKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = 0m,
                        TaxAmount = 0m,
                        LineTotal = item.LineTotal
                    });
                }
                context.Invoices.Add(invoice);
            }
        }
        context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = id, Status = order.Status, ChangedAt = DateTimeOffset.UtcNow, Note = verified ? "تم التحقق من استلام الدفع." : "تم تسجيل بيانات الدفع (بانتظار التحقق)." });
        await context.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ بيانات الدفع.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveShipment(Guid id, string carrier, string? trackingNumber, string? status, string? shippedAt, string? deliveredAt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(carrier)) { TempData["Error"] = "يجب تحديد شركة/وسيلة التوصيل."; return RedirectToAction(nameof(Details), new { id }); }
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        if (order.Status == OrderStatuses.Cancelled) { TempData["Error"] = "لا يمكن تسجيل شحن لطلب ملغي."; return RedirectToAction(nameof(Details), new { id }); }

        var shipment = await context.Shipments.FirstOrDefaultAsync(x => x.OrderId == id, ct);
        var isNew = shipment is null;
        if (isNew) shipment = new Shipment { OrderId = id };
        shipment!.Carrier = carrier.Trim();
        shipment.TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
        shipment.Status = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();
        if (DateTimeOffset.TryParse(shippedAt, out var sd)) shipment.ShippedAt = sd;
        else if (shipment.Status == "Shipped" && shipment.ShippedAt is null) shipment.ShippedAt = DateTimeOffset.UtcNow;
        if (DateTimeOffset.TryParse(deliveredAt, out var dd)) shipment.DeliveredAt = dd;
        else if (shipment.Status == "Delivered" && shipment.DeliveredAt is null) shipment.DeliveredAt = DateTimeOffset.UtcNow;
        if (isNew) context.Shipments.Add(shipment);
        context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId = id, Status = order.Status, ChangedAt = DateTimeOffset.UtcNow, Note = $"تم حفظ بيانات الشحن ({shipment.Carrier})." });
        await context.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ بيانات الشحن.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Invoice(Guid id, CancellationToken ct)
    {
        var order = await context.Orders.AsNoTracking().Include(x => x.Items).Include(x => x.ShippingAddress).Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound();
        var invoice = await context.Invoices.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.OrderId == id, ct);
        if (invoice is null) return NotFound();
        ViewBag.Order = order;
        ViewBag.Payment = await context.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == id, ct);
        ViewBag.Customer = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.CustomerId, ct);
        return View(invoice);
    }
    private static bool PaymentSatisfied(IEnumerable<Payment> payments)
    {
        var list = payments.ToList();
        var required = list.Max(p => (decimal?)p.RequiredAmount) ?? 0m;
        var confirmed = list.Where(p => p.Status == PaymentStatuses.Paid).Sum(p => p.Amount);
        return required > 0 && confirmed >= required;
    }

    private static List<SelectListItem> Statuses() => [new("قيد الانتظار", OrderStatuses.Pending),new("قيد التجهيز", OrderStatuses.Processing),new("تم الشحن", OrderStatuses.Shipped),new("تم التسليم", OrderStatuses.Delivered),new("ملغي", OrderStatuses.Cancelled)];
    private static bool CanTransition(string from, string to) => from == to || (from, to) switch
    {
        (OrderStatuses.Pending, OrderStatuses.Processing or OrderStatuses.Cancelled) => true,
        (OrderStatuses.Processing, OrderStatuses.Shipped or OrderStatuses.Cancelled) => true,
        (OrderStatuses.Shipped, OrderStatuses.Delivered) => true,
        _ => false
    };
}
