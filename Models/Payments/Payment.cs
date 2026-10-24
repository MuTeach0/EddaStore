using EddaStore.Models.Common;
using EddaStore.Models.Orders;

namespace EddaStore.Models.Payments;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";

    public string? TransactionReference { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}
