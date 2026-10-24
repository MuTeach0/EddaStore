
using EddaStore.Data;
using EddaStore.Models.ViewModels.Admin.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = new DashboardViewModel
        {
            // المنتجات والمخزون
            TotalProducts = await _context.Products
                .CountAsync(cancellationToken),

            ActiveProducts = await _context.Products
                .CountAsync(p => p.IsActive, cancellationToken),

            InactiveProducts = await _context.Products
                .CountAsync(p => !p.IsActive, cancellationToken),

            OutOfStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity == 0, cancellationToken),

            LowStockProducts = await _context.Products
                .CountAsync(
                    p => p.StockQuantity > 0 && p.StockQuantity <= 5,
                    cancellationToken),

            // التصنيفات والماركات
            TotalCategories = await _context.Categories
                .CountAsync(cancellationToken),

            TotalBrands = await _context.Brands
                .CountAsync(cancellationToken),

            // إجمالي الطلبات
            TotalOrders = await _context.Orders
                .CountAsync(cancellationToken),

            PendingOrders = await _context.Orders.CountAsync(x => x.Status == "Pending", cancellationToken),
            ProcessingOrders = await _context.Orders.CountAsync(x => x.Status == "Processing", cancellationToken),
            ShippedOrders = await _context.Orders.CountAsync(x => x.Status == "Shipped", cancellationToken),
            DeliveredOrders = await _context.Orders.CountAsync(x => x.Status == "Delivered", cancellationToken),
            CancelledOrders = await _context.Orders.CountAsync(x => x.Status == "Cancelled", cancellationToken),

            TotalPayments = await _context.Payments.CountAsync(cancellationToken),
            PendingPayments = await _context.Payments.CountAsync(x => x.Status == "Pending", cancellationToken),
            SuccessfulPayments = await _context.Payments.CountAsync(x => x.Status == "Paid" || x.Status == "Successful", cancellationToken),
            TotalRevenue = await _context.Payments.Where(x => x.Status == "Paid" || x.Status == "Successful").SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0,

            TotalShipments = await _context.Shipments.CountAsync(cancellationToken),
            PendingShipments = await _context.Shipments.CountAsync(x => x.Status == "Pending", cancellationToken),
            DeliveredShipments = await _context.Shipments.CountAsync(x => x.Status == "Delivered", cancellationToken),

            TotalReviews = await _context.Reviews.CountAsync(cancellationToken),
            PendingReviews = await _context.Reviews.CountAsync(x => !x.IsApproved, cancellationToken),

            // أحدث الطلبات
            RecentOrders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.PlacedAt)
                .ThenByDescending(o => o.Id)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    Currency = o.Currency,
                    PlacedAt = o.PlacedAt
                })
                .ToListAsync(cancellationToken)
        };

        return View(model);
    }
}
