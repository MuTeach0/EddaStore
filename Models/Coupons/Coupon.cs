
using EddaStore.Models.Common;

namespace EddaStore.Models.Coupons;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Percentage أو FixedAmount
    public string DiscountType { get; set; } = "Percentage";

    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }

    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }

    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}
