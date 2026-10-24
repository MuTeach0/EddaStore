using EddaStore.Constants;
using EddaStore.Data;
using EddaStore.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EddaStore.Models.Payments;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? status, string? search, CancellationToken ct)
    {
        var query = context.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.OrderNumber.Contains(search));
        ViewBag.Statuses = Statuses(); ViewBag.Status = status; ViewBag.Search = search;
        return View(await query.OrderByDescending(x => x.PlacedAt).ToListAsync(ct));
    }
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await context.Orders.AsNoTracking().Include(x => x.Items).Include(x => x.StatusHistory).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (order is null) return NotFound(); ViewBag.Statuses = Statuses(); return View(order);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status, CancellationToken ct)
    {
        var order = await context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct); if (order is null) return NotFound();
        if (!CanTransition(order.Status, status)) { TempData["Error"] = "انتقال حالة الطلب غير مسموح."; return RedirectToAction(nameof(Details), new { id }); }
        if (order.Status != status)
        {
            if (status == OrderStatuses.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await context.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId, ct);
                    if (product is not null) product.StockQuantity += item.Quantity;
                }
                foreach (var payment in await context.Payments.Where(x => x.OrderId == id).ToListAsync(ct)) payment.Status = "Cancelled";
            }
            if (status == OrderStatuses.Delivered)
                foreach (var payment in await context.Payments.Where(x => x.OrderId == id && x.Provider == "CashOnDelivery").ToListAsync(ct)) { payment.Status = "Paid"; payment.PaidAt = DateTimeOffset.UtcNow; }
            context.OrderStatusHistories.Add(new OrderStatusHistory { OrderId=id, Status=status, ChangedAt=DateTimeOffset.UtcNow }); order.Status=status; await context.SaveChangesAsync(ct);
        }
        TempData["Success"]="تم تحديث حالة الطلب."; return RedirectToAction(nameof(Details), new { id });
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
