namespace EddaStore.Models.ViewModels.Admin.Reports;

public class SalesReportViewModel
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int Customers { get; set; }
    public List<TopSellingProductViewModel> TopProducts { get; set; } = new();
}

public class TopSellingProductViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}