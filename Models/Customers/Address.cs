
using EddaStore.Models.Common;

namespace EddaStore.Models.Customers;

public class Address : BaseEntity
{
    public Guid CustomerId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Country { get; set; } = "Egypt";
    public string City { get; set; } = string.Empty;
    public string? StateOrGovernorate { get; set; }
    public string StreetAddress { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Apartment { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}