using EddaStore.Models.Common;
using EddaStore.Models.Identity;
using EddaStore.Models.Products;

namespace EddaStore.Models.Customers;

public class WishlistItem : BaseEntity
{
    public Guid CustomerId { get; set; }
    public ApplicationUser Customer { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
