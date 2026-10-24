
using EddaStore.Models.Common;
using EddaStore.Models.Orders;

namespace EddaStore.Models.Invoices;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public DateTimeOffset IssuedAt { get; set; }

    public string Status { get; set; } = "Issued";

    public string Currency { get; set; } = "EGP";

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public ICollection<InvoiceItem> Items { get; set; }
        = new List<InvoiceItem>();
}
