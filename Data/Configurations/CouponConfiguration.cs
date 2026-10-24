using EddaStore.Models.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EddaStore.Data.Configurations;

// Coupons
public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DiscountType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.MinimumOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.MaximumDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Coupons_DiscountValue",
            "[DiscountValue] > 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Coupons_Usage",
            "[UsageCount] >= 0 AND ([UsageLimit] IS NULL OR [UsageLimit] >= 0)"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Coupons_DateRange",
            "[ExpiresAt] >= [StartsAt]"));
    }
}
