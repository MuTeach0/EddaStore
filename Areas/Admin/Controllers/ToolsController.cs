using EddaStore.Data;
using EddaStore.Models.ViewModels.Admin.Reports;
using EddaStore.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Areas.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class ToolsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager) : Controller
{
    public async Task<IActionResult> Inventory(CancellationToken ct) => View(await context.Products.AsNoTracking().Include(x => x.Category).OrderBy(x => x.StockQuantity).ToListAsync(ct));
    public async Task<IActionResult> Customers(CancellationToken ct) => View(await userManager.Users.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct));
    public async Task<IActionResult> Roles(CancellationToken ct) => View(await roleManager.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct));
    public async Task<IActionResult> Reports(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var fromDate = from.HasValue
            ? new DateTimeOffset(from.Value.Date, TimeSpan.Zero)
            : DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = to.HasValue
            ? new DateTimeOffset(to.Value.Date.AddDays(1), TimeSpan.Zero)
            : DateTimeOffset.UtcNow.AddDays(1);

        var model = new SalesReportViewModel
        {
            From = fromDate,
            To = toDate.AddDays(-1),
            TotalRevenue = await context.Orders
                .Where(x => x.Status == "Delivered" && x.PlacedAt >= fromDate && x.PlacedAt < toDate)
                .SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0,
            TotalOrders = await context.Orders
                .CountAsync(x => x.Status != "Cancelled" && x.PlacedAt >= fromDate && x.PlacedAt < toDate, ct),
            CancelledOrders = await context.Orders
                .CountAsync(x => x.Status == "Cancelled" && x.PlacedAt >= fromDate && x.PlacedAt < toDate, ct),
            Customers = await userManager.Users.CountAsync(ct)
        };

        var top = await context.OrderItems.AsNoTracking()
            .Where(x => x.Order.Status == "Delivered" && x.Order.PlacedAt >= fromDate && x.Order.PlacedAt < toDate)
            .GroupBy(x => x.ProductName)
            .Select(g => new TopSellingProductViewModel
            {
                Name = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToListAsync(ct);

        model.TopProducts = top;
        return View(model);
    }
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        ViewBag.PendingOrders = await context.Orders.CountAsync(x => x.Status == "Pending", ct);
        ViewBag.LowStock = await context.Products.CountAsync(x => x.StockQuantity > 0 && x.StockQuantity <= 5, ct);
        ViewBag.PendingReviews = await context.Reviews.CountAsync(x => !x.IsApproved, ct);
        return View();
    }
    public async Task<IActionResult> Search(string? q, CancellationToken ct)
    {
        q = q?.Trim();
        ViewBag.Query = q;
        if (string.IsNullOrWhiteSpace(q))
        {
            return View();
        }

        ViewBag.Products = await context.Products.AsNoTracking()
            .Where(x => x.Name.Contains(q) || (x.SKU != null && x.SKU.Contains(q)))
            .Take(20).ToListAsync(ct);
        ViewBag.Orders = await context.Orders.AsNoTracking()
            .Where(x => x.OrderNumber.Contains(q))
            .Take(20).ToListAsync(ct);
        ViewBag.Customers = await userManager.Users.AsNoTracking()
            .Where(x => x.Email != null && x.Email.Contains(q))
            .Take(20).ToListAsync(ct);
        return View();
    }
    public IActionResult Settings() => View();
}
