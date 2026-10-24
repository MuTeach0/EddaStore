
using EddaStore.Models.Common;

namespace EddaStore.Models.Customers;

public class Cart : BaseEntity
{
    public Guid CustomerId { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
