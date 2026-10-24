
using EddaStore.Models.Common;
using EddaStore.Models.Coupons;
using EddaStore.Models.Customers;
using EddaStore.Models.Identity;
using EddaStore.Models.Invoices;
using EddaStore.Models.Orders;
using EddaStore.Models.Payments;
using EddaStore.Models.Products;
using EddaStore.Models.Shipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EddaStore.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{

    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
    public DbSet<OrderStatusHistory> OrderStatusHistories
        => Set<OrderStatusHistory>();

    // Customers
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Coupons
    public DbSet<Coupon> Coupons => Set<Coupon>();

    // Payments and shipping
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    // Invoices
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> implementations.
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        // Apply global soft-delete filters to ISoftDelete entities.
        foreach (var entityType in builder.Model.GetEntityTypes().ToList())
        {
            if (entityType.IsOwned() ||
                !typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ApplicationDbContext).GetMethod(
                nameof(ApplySoftDeleteFilter),
                BindingFlags.NonPublic | BindingFlags.Static)!;

            method.MakeGenericMethod(entityType.ClrType)
                .Invoke(null, new object[] { builder });
        }

        // Orders: preserve order history and related records.
        builder.Entity<Order>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasMany(x => x.StatusHistory)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Products: preserve images when a product is referenced.
        builder.Entity<Product>()
            .HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice: one invoice per order in the initial design.
        builder.Entity<Invoice>()
            .HasOne(x => x.Order)
            .WithOne()
            .HasForeignKey<Invoice>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ApplySoftDeleteFilter<TEntity>(
        ModelBuilder builder)
        where TEntity : class, ISoftDelete
    {
        builder.Entity<TEntity>()
            .HasQueryFilter(entity => !entity.IsDeleted);
    }

    // Convert Remove() into a soft delete for ISoftDelete entities.
    public override int SaveChanges()
    {
        ApplySoftDeletes();

        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDeletes();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplySoftDeletes();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDeletes();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ApplySoftDeletes()
    {
        ChangeTracker.DetectChanges();

        var deletedEntries = ChangeTracker
            .Entries<ISoftDelete>()
            .Where(entry => entry.State == EntityState.Deleted)
            .ToList();

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;

            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
        }
    }
}
