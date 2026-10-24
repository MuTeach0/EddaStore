using EddaStore.Data;
using EddaStore.Models.Identity;
using EddaStore.Models.Orders;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Reports;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Services;

public sealed class AdminToolsService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : IAdminToolsService
{
    public async Task<IReadOnlyList<Product>> GetInventoryAsync(CancellationToken ct = default)
        => await context.Products.AsNoTracking().Include(product => product.Category)
            .OrderBy(product => product.StockQuantity).ToListAsync(ct);

    public async Task<IReadOnlyList<ApplicationUser>> GetCustomersAsync(CancellationToken ct = default)
        => await userManager.Users.AsNoTracking().OrderByDescending(user => user.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesAsync(CancellationToken ct = default)
        => await roleManager.Roles.AsNoTracking().OrderBy(role => role.Name).ToListAsync(ct);

    public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var fromDate = from.HasValue ? new DateTimeOffset(from.Value.Date, TimeSpan.Zero) : DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = to.HasValue ? new DateTimeOffset(to.Value.Date.AddDays(1), TimeSpan.Zero) : DateTimeOffset.UtcNow.AddDays(1);
        var model = new SalesReportViewModel
        {
            From = fromDate, To = toDate.AddDays(-1),
            TotalRevenue = await context.Orders.Where(order => order.Status == "Delivered" && order.PlacedAt >= fromDate && order.PlacedAt < toDate).SumAsync(order => (decimal?)order.TotalAmount, ct) ?? 0,
            TotalOrders = await context.Orders.CountAsync(order => order.Status != "Cancelled" && order.PlacedAt >= fromDate && order.PlacedAt < toDate, ct),
            CancelledOrders = await context.Orders.CountAsync(order => order.Status == "Cancelled" && order.PlacedAt >= fromDate && order.PlacedAt < toDate, ct),
            Customers = await userManager.Users.CountAsync(ct)
        };
        model.TopProducts = await context.OrderItems.AsNoTracking()
            .Where(item => item.Order.Status == "Delivered" && item.Order.PlacedAt >= fromDate && item.Order.PlacedAt < toDate)
            .GroupBy(item => item.ProductName)
            .Select(group => new TopSellingProductViewModel { Name = group.Key, Quantity = group.Sum(item => item.Quantity), Revenue = group.Sum(item => item.LineTotal) })
            .OrderByDescending(item => item.Quantity).Take(10).ToListAsync(ct);
        return model;
    }

    public async Task<(int PendingOrders, int LowStock, int PendingReviews)> GetNotificationCountsAsync(CancellationToken ct = default)
        => (await context.Orders.CountAsync(order => order.Status == "Pending", ct),
            await context.Products.CountAsync(product => product.StockQuantity > 0 && product.StockQuantity <= 5, ct),
            await context.Reviews.CountAsync(review => !review.IsApproved, ct));

    public async Task<ToolsSearchViewModel> SearchAsync(string? query, CancellationToken ct = default)
    {
        query = query?.Trim();
        var result = new ToolsSearchViewModel { Query = query };
        if (string.IsNullOrWhiteSpace(query)) return result;
        result.Products = await context.Products.AsNoTracking().Where(product => product.Name.Contains(query) || (product.SKU != null && product.SKU.Contains(query))).Take(20).ToListAsync(ct);
        result.Orders = await context.Orders.AsNoTracking().Where(order => order.OrderNumber.Contains(query)).Take(20).ToListAsync(ct);
        result.Customers = await userManager.Users.AsNoTracking().Where(user => user.Email != null && user.Email.Contains(query)).Take(20).ToListAsync(ct);
        return result;
    }
}
