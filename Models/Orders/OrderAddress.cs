
using EddaStore.Models.Common;

namespace EddaStore.Models.Orders;

public class OrderAddress : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "Egypt";
    public string City { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Apartment { get; set; }
    public string? PostalCode { get; set; }
}
