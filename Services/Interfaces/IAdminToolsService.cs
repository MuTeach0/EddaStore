using EddaStore.Models.Identity;
using EddaStore.Models.Orders;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Reports;
using Microsoft.AspNetCore.Identity;

namespace EddaStore.Services.Interfaces;

public interface IAdminToolsService
{
    Task<IReadOnlyList<Product>> GetInventoryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdentityRole<Guid>>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<SalesReportViewModel> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<(int PendingOrders, int LowStock, int PendingReviews)> GetNotificationCountsAsync(CancellationToken cancellationToken = default);
    Task<ToolsSearchViewModel> SearchAsync(string? query, CancellationToken cancellationToken = default);
}
