
using EddaStore.Models.Common;
using EddaStore.Models.Invoices;
using EddaStore.Models.Payments;
using EddaStore.Models.Shipping;

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
    public OrderAddress? ShippingAddress { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Shipment? Shipment { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
