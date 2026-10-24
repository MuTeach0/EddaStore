using System.ComponentModel.DataAnnotations;

namespace EddaStore.Models.ViewModels.Store;

public class CheckoutInputViewModel
{
    [Required, StringLength(200)] public string RecipientName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [Required, StringLength(300)] public string StreetAddress { get; set; } = string.Empty;
    [StringLength(50)] public string? Building { get; set; }
    [StringLength(50)] public string? Apartment { get; set; }
    [StringLength(20)] public string? PostalCode { get; set; }
    [StringLength(50)] public string? CouponCode { get; set; }
    [Required, StringLength(64)] public string CheckoutToken { get; set; } = Guid.NewGuid().ToString("N");
}

public class CheckoutPageViewModel
{
    public CheckoutInputViewModel Input { get; set; } = new();
    public CartViewModel Cart { get; set; } = new();
}
