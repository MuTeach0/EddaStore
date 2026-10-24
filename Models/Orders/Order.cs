
using EddaStore.Models.Common;

namespace EddaStore.Models.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public string CheckoutToken { get; set; } = string.Empty;
    public string? CouponCode { get; set; }

    public Guid CustomerId { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "EGP";

    public DateTimeOffset? PlacedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
