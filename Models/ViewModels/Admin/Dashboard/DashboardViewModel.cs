
namespace EddaStore.Models.ViewModels.Admin.Dashboard;

public class DashboardViewModel
{
    // المنتجات والمخزون
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }

    // التصنيفات والماركات
    public int TotalCategories { get; set; }
    public int TotalBrands { get; set; }

    // الطلبات
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }

    // المدفوعات
    public int TotalPayments { get; set; }
    public int PendingPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public decimal TotalRevenue { get; set; }

    // الشحن
    public int TotalShipments { get; set; }
    public int PendingShipments { get; set; }
    public int DeliveredShipments { get; set; }

    // المراجعات
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }

    // بيانات حديثة للعرض
    public List<RecentOrderViewModel> RecentOrders { get; set; } = [];
}

public class RecentOrderViewModel
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public DateTimeOffset? PlacedAt { get; set; }
}
