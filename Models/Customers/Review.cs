using EddaStore.Models.Common;

namespace EddaStore.Models.Customers;

public class Review : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
}
