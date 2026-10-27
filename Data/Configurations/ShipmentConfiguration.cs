using EddaStore.Models.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EddaStore.Data.Configurations;

// Shipments
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Carrier)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(150);

        builder.Property(x => x.Status)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Shipment)
            .HasForeignKey<Shipment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => new { x.Carrier, x.TrackingNumber });
    }
}