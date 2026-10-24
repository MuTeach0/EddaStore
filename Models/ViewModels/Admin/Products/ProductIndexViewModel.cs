
using EddaStore.Models.Products;

namespace EddaStore.Models.ViewModels.Admin.Products;

public class ProductIndexViewModel
{
    public IReadOnlyList<Product> Products { get; set; }
        = [];

    public string? SearchTerm { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int TotalCount { get; set; }
}
