
using EddaStore.Models.Common;

namespace EddaStore.Models.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }

    // Snapshot لبيانات المنتج وقت الشراء
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
