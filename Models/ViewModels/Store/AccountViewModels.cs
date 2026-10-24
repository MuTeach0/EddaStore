using EddaStore.Models.Customers;
using EddaStore.Models.Orders;

namespace EddaStore.Models.ViewModels.Store;

public class AccountViewModel
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int WishlistCount { get; set; }
    public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
    public IReadOnlyList<Address> Addresses { get; set; } = Array.Empty<Address>();
}
