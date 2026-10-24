using EddaStore.Models.Identity;
using EddaStore.Models.Orders;
using EddaStore.Models.Products;

namespace EddaStore.Models.ViewModels.Admin.Reports;

public sealed class ToolsSearchViewModel
{
    public string? Query { get; set; }
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
    public IReadOnlyList<ApplicationUser> Customers { get; set; } = Array.Empty<ApplicationUser>();
}
