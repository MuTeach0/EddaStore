using EddaStore.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EddaStore.Data.Configurations;

// Order address snapshot
public class OrderAddressConfiguration
    : IEntityTypeConfiguration<OrderAddress>
{
    public void Configure(EntityTypeBuilder<OrderAddress> builder)
    {
        builder.ToTable("OrderAddresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.StreetAddress)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Building)
            .HasMaxLength(50);

        builder.Property(x => x.Apartment)
            .HasMaxLength(50);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Order)
            .WithOne()
            .HasForeignKey<OrderAddress>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}
