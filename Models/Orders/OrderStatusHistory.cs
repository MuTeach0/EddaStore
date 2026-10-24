
using EddaStore.Models.Common;

namespace EddaStore.Models.Orders;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public Guid? ChangedByUserId { get; set; }
}
