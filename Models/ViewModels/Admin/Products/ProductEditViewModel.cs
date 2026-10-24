
using System.ComponentModel.DataAnnotations;
using EddaStore.Models.Products;

namespace EddaStore.Models.ViewModels.Admin.Products;

public class ProductEditViewModel : ProductCreateViewModel
{
    [Required]
    public Guid Id { get; set; }

    public IReadOnlyList<ProductImage> ExistingImages { get; set; } = Array.Empty<ProductImage>();
}
